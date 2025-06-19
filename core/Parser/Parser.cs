using Rich.Diagnostics;
using Rich.Lexer;
using Rich.Parser.SyntaxNodes;
using Rich.Parser.SyntaxNodes.Expressions;
using Rich.Parser.SyntaxNodes.Extensions;

namespace Rich.Parser;

public partial class Parser
{
    private List<Token> _tokens = null!;
    private int _current;
    private Token Token => _tokens[_current];
    private bool IsEndOfTokens => Token.Type is TokenType.EndOfFile;
    private bool IsFunctionCall => IsPossibleType && (Peek()?.Type is TokenType.OpenParenthesis || Peek()?.Type is TokenType.LessThan);
    private bool IsEndOfStatement => Token.Type is TokenType.EndOfLine or TokenType.EndOfFile or TokenType.CloseBracket;
    private bool IsIndexor => IsPossibleType && Peek()?.Type is TokenType.OpenSquareBracket;
    private bool IsPossibleType => Token.Type is TokenType.Identifier or TokenType.BuiltInType;
    
    public SyntaxTree Run(List<Token> tokens)
    {
        _tokens = tokens;
        var syntaxTree = new SyntaxTree
        {
            FilePath = _tokens[0].Span.FilePath,
            Root = ParseBlock(BlockType.TopLevel)
        };

        return syntaxTree;
    }
    
    /// <summary>
    /// By default, <see cref="MoveNext"/> ignores new lines and whitespace.
    /// To stop on whitespace or new lines or both, pass a <see cref="MoveInclude"/>.
    /// </summary>
    /// <param name="moveInclude">Determines what special tokens are valid to move to.</param>
    /// <returns></returns>
    private bool MoveNext(MoveInclude moveInclude = MoveInclude.None)
    {
        if (_current + 1 >= _tokens.Count) return false;

        _current++;

        Func<bool> predicate = moveInclude switch
        {
            MoveInclude.None => () => 
                Token.Type is TokenType.Whitespace or TokenType.EndOfLine,
            MoveInclude.NewLines => () => Token.Type is TokenType.Whitespace,
            MoveInclude.Whitespace => () => Token.Type is TokenType.EndOfLine,
            MoveInclude.Whitespace | MoveInclude.NewLines => () => false,
            _ => () =>
                Token.Type is TokenType.Whitespace or TokenType.EndOfLine
        };

        while (_current < _tokens.Count - 1 && (predicate() || Token.Type is TokenType.Comment))
        {
            _current++;
        }
        
        return true;
    }

    /// <summary>
    /// Only moves to the next token when the current
    /// token is an <see cref="TokenType.EndOfLine"/>.
    /// </summary>
    private void MoveNextIfEndOfLine()
    {
        if (Token.Type is TokenType.EndOfLine) MoveNext();
    }
    
    /// <summary>
    /// Peeks the next non-whitespace and non-newline token.
    /// </summary>
    /// <returns></returns>
    private Token? Peek()
    {
        if (_current + 1 >= _tokens.Count) return null;

        var index = _current + 1;

        while (index < _tokens.Count - 1
               && _tokens[index].Type is TokenType.Whitespace or TokenType.EndOfLine)
        {
            index++;
        }
        
        return _tokens[index];
    }

    private void AssertToken(TokenType expected, string message)
    {
        if (Token.Type == expected) return;
        Report.Error(message, Token.Span);
    }
    
    private void AssertToken(List<TokenType> expected, string message)
    {
        if (expected.Any(x => x == Token.Type)) return;
        Report.Error(message, Token.Span);
    }

    private void UnsertToken(TokenType unexpected, string message)
    {
        if (Token.Type != unexpected) return;
        Report.Error(message, Token.Span);
    }

    /// <summary>
    /// Parses a block.
    /// </summary>
    private BlockSyntax ParseBlock(BlockType blockType)
    {
        var blockSyntax = new BlockSyntax(blockType);
        
        switch (blockType)
        {
            case BlockType.Block:
                if (Token.Type is not TokenType.OpenBracket)
                {
                    Report.Error("Block must begin with {.", Token.Span);
                }
                MoveNext();
                
                while (!IsEndOfTokens && Token.Type is not TokenType.CloseBracket) InnerParseStatement();

                if (Token.Type is not TokenType.CloseBracket)
                {
                    Report.Error("Block must end with }.", Token.Span);
                }
                MoveNext(MoveInclude.NewLines);
                break;
            case BlockType.TopLevel: while (!IsEndOfTokens) InnerParseStatement();
                break;
        }
        
        return blockSyntax;

        void InnerParseStatement()
        {
            if (ParseStatement() is { } node)
            {
                blockSyntax.Children.Add(node);
            }
            else
            {
                MoveNext();
            }
        }
    }
    
    /// <summary>
    /// This is the most called parse method.<br/>
    /// All other parse methods except for
    /// <see cref="ParseBlock"/> are called from here.
    /// </summary>
    private Syntax? ParseStatement()
    {
        switch (Token.Type)
        {
            case TokenType.KeywordType:
            {
                var typeDefinition = ParseTypeDefinition();
                AssertEndOfLine();
                return typeDefinition;
            }
            case TokenType.KeywordFunction:
            {
                var function = ParseFunction();
                AssertEndOfLine();
                return function;
            }
            case TokenType.KeywordReturn:
            {
                var @return = ParseReturn();
                AssertEndOfLine();
                return @return;
            }
            case TokenType.KeywordBreak:
            {
                var @break = ParseBreak();
                AssertEndOfLine();
                return @break;
            }
            case TokenType.KeywordContinue:
            {
                var @continue = ParseContinue();
                AssertEndOfLine();
                return @continue;
            }
            case TokenType.KeywordWhile:
            {
                var @while = ParseWhile();
                AssertEndOfLine();
                return @while;
            }
            case TokenType.KeywordIf:
            {
                var @if = ParseIf();
                AssertEndOfLine();
                return @if;
            }
            case TokenType.KeywordImport:
            {
                var import = ParseImport();
                AssertEndOfLine();
                return import;
            }
            case TokenType.KeywordPath:
            {
                var path = ParsePath();
                AssertEndOfLine();
                return path;
            }
            // make sure this statement is always at the bottom
            // this will ensure that this statement won't take
            // priority if other statements require and identifier
            // as their first token
            case TokenType.Identifier:
            {
                var statement = ParseStatementThatStartsWithIdentifier();
                AssertEndOfLine();
                return statement;
            }
            case TokenType.BuiltInType:
                Report.Error("Type should not be here.", Token.Span);
                break;
        }

        if (Token.Type is TokenType.Whitespace or TokenType.EndOfLine or TokenType.Comment) return null;
        
        Report.Error("Invalid statement.", Token.Span);
        return null;

        void AssertEndOfLine()
        {
            if (!IsEndOfStatement) Report.Error("Statements must end with a new line.", Token.Span);
            MoveNextIfEndOfLine();
        }
    }
    
    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private FunctionSyntax ParseFunction()
    {
        AssertToken(TokenType.KeywordFunction, "Expected a function keyword.");
        MoveNext();
        
        AssertToken(TokenType.Identifier, "Function is missing a name.");
        var function = new FunctionSyntax(new IdentifierSyntax(Token.Span));
        
        MoveNext();

        // if this function has generics
        if (Token.Type is TokenType.LessThan)
        {
            function.TypeParameterList = ParseGenericsListDefinition();
            MoveNext();
        }
        
        AssertToken(TokenType.OpenParenthesis, "Function is missing (.");
        MoveNext();
        
        if (Token.Type is not TokenType.CloseParenthesis)
        {
            while (true)
            {
                AssertToken(TokenType.Identifier, "Expected a parameter name.");
                var parameterNameSpan = Token.Span;
                
                MoveNext();
                
                AssertToken(TokenType.Colon, "Expected a colon between the parameter name and its type.");
                MoveNext();
                
                var parameter = new ParameterSyntax(parameterNameSpan, ParseType());
                MoveNext();
                function.Parameters.Add(parameter);

                if (Token.Type is TokenType.CloseParenthesis) break;
                AssertToken(TokenType.Comma, "Expected a comma between parameters.");
                if (!MoveNext()) break;
            }
            
            AssertToken(TokenType.CloseParenthesis, "Function is missing ).");
        }

        MoveNext();
        
        // the function may have a return type
        if (Token.Type is TokenType.Colon)
        {
            MoveNext();
            function.ReturnType = ParseType();
            MoveNext();
        }
        
        AssertToken(TokenType.OpenBracket, "Function should have a body but is missing {.");
        function.Body = ParseBlock(BlockType.Block);
        return function;
    }
    
    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached most of the time.
    /// </summary>
    private Syntax? ParseStatementThatStartsWithIdentifier()
    {
        var accessorChain = ParseAccessorChain();
        
        // this must be a variable declaration
        if (Peek()?.Type is TokenType.Colon)
        {
            MoveNext();
            
            // only an identifier is allowed on the left
            // side of a variable declaration
            if (accessorChain?.Chain.FirstOrDefault() is not IdentifierSyntax accessorChainForReal)
            {
                Report.Error("Expected the name of a variable.", Token.Span);
                return null;
            }
            
            MoveNext();

            TypeSyntax? typeSyntax = null;
            
            // is a type specified for this variable declaration
            if (Token.Type is TokenType.BuiltInType or TokenType.Identifier)
            {
                typeSyntax = ParseType();

                if (Peek()?.Type is TokenType.Assignment)
                {
                    MoveNext();
                }
                else
                {
                    MoveNext(MoveInclude.NewLines);
                    return new VariableDeclarationSyntax(
                        accessorChainForReal,
                        typeSyntax,
                        null);
                }
            }
            // if not, ensure that there is an assignment
            // assuming that this is an auto variable declaration
            else if (Token.Type is not TokenType.Assignment)
            {
                Report.Error("Auto variables require an assignment.", Token.Span);
            }
            
            // parse the assignment of the variable declaration
            if (Token.Type is TokenType.Assignment)
            {
                MoveNext();
                var declaration = new VariableDeclarationSyntax(
                    accessorChainForReal,
                    typeSyntax,
                    ParseExpression());
                
                MoveNext(MoveInclude.NewLines);
                return declaration;
            }
        }
        // this must be a variable assignment
        else if (Peek()?.Type is TokenType.Assignment)
        {
            MoveNext();
            MoveNext();
            var expression = ParseExpression();
            MoveNext(MoveInclude.NewLines);
            return new BinaryOperatorSyntax(BinaryOperatorKind.Assignment)
            {
                Left = accessorChain,
                Right = expression
            };
        }
        // this must be a function call or just simply an accessor chain 
        else if (accessorChain.IsRightMostAFunctionCall())
        {
            MoveNext(MoveInclude.NewLines);
            return accessorChain;
        }
            
        Report.Error("A solitary accessor chain is not allowed.", Token.Span);
        return null;
    }
    
    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private ReturnSyntax ParseReturn()
    {
        MoveNext();
        var @return = new ReturnSyntax(ParseExpression());
        MoveNext(MoveInclude.NewLines);
        return @return;
    }

    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private BreakSyntax ParseBreak()
    {
        MoveNext(MoveInclude.NewLines);
        return new BreakSyntax();
    }
    
    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private ContinueSyntax ParseContinue()
    {
        MoveNext(MoveInclude.NewLines);
        return new ContinueSyntax();
    }

    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private WhileSyntax ParseWhile()
    {
        MoveNext();
        var condition = ParseExpression();
        MoveNext();
        
        if (Token.Type is not TokenType.OpenBracket) Report.Error("While statement does not have a body because it is missing {.", Token.Span);

        var body = ParseBlock(BlockType.Block);
        
        return new WhileSyntax(condition)
        {
            Body = body
        };
    }

    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private IfSyntax ParseIf()
    {
        MoveNext();
        var condition = ParseExpression();
        MoveNext();
        
        if (Token.Type is not TokenType.OpenBracket) Report.Error("If statement does not have a body because it is missing {.", Token.Span);
        
        var body = ParseBlock(BlockType.Block);
        
        var @if = new IfSyntax(condition)
        {
            Body = body
        };

        // ensures that after reading the block
        // if we landed on a new line token
        // that the next token is an else token
        // if so, we advance to that else token
        if (Token.Type is TokenType.EndOfLine && Peek()?.Type is TokenType.KeywordElse) MoveNext();
        
        if (Token.Type is TokenType.KeywordElse)
        {
            MoveNext();

            if (Token.Type is TokenType.KeywordIf)
            {
                @if.Branch = ParseIf();
            }
            else if (Token.Type is TokenType.OpenBracket)
            {
                @if.Branch = new ElseSyntax
                {
                    Body = ParseBlock(BlockType.Block)
                };
                
            }
            else
            {
                Report.Error("Invalid if conditional branch.", Token.Span);
            }
        }

        return @if;
    }

    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private ImportSyntax? ParseImport()
    {
        MoveNext();
        var accessorChain = ParseAccessorChain();
        ImportSyntax? import = null;

        if (accessorChain is not null)
        {
            import = new ImportSyntax(accessorChain);

            if (!import.AccessorChainChain.IsOnlyIdentifiers())
            {
                Report.Error("Invalid import.", Token.Span);
            }
        }
        else
        {
            Report.Error("Import could not be found.", Token.Span);
        }
        
        MoveNext(MoveInclude.NewLines);

        return import;
    }

    private PathSyntax? ParsePath()
    {
        MoveNext();
        var accessorChain = ParseAccessorChain();
        PathSyntax? path = null;

        if (accessorChain is not null)
        {
            path = new PathSyntax(accessorChain);

            if (!path.AccessorChainChain.IsOnlyIdentifiers())
            {
                Report.Error("Invalid path.", Token.Span);
            }
        }
        else
        {
            Report.Error("Path could not be found.", Token.Span);
        }
        
        MoveNext(MoveInclude.NewLines);

        return path;
    }

    /// <summary>
    /// Ensures <see cref="TokenType.EndOfLine"/> is reached.
    /// </summary>
    private TypeDefinitionSyntax ParseTypeDefinition()
    {
        MoveNext();
        
        if (Token.Type is not TokenType.Identifier) Report.Error("Expected type definition name.", Token.Span);
        var identifierSpan = Token.Span;

        MoveNext();

        TypeParameterListSyntax? genericsListDefinitionSyntax = null;

        if (Token.Type is TokenType.LessThan)
        {
            genericsListDefinitionSyntax = ParseGenericsListDefinition();
            MoveNext();
        }

        if (Token.Type is not TokenType.OpenBracket) Report.Error("Type definition does not have a body because it is missing {.", Token.Span);

        MoveNext();

        var typeDefinition = new TypeDefinitionSyntax(new IdentifierSyntax(identifierSpan));

        while (!IsEndOfTokens && Token.Type is not TokenType.CloseBracket)
        {
            if (Token.Type is TokenType.Identifier)
            {
                if (ParseStatementThatStartsWithIdentifier() is VariableDeclarationSyntax variableDeclaration)
                {
                    typeDefinition.Variables.Add(variableDeclaration);
                }
                else
                {
                    Report.Error("Expected variable declaration.", Token.Span);
                }
            }
            else if (Token.Type is TokenType.KeywordFunction)
            {
                typeDefinition.Functions.Add(ParseFunction());
            }
            else
            {
                Report.Error("Expected function or variable declaration.", Token.Span);
            }
            MoveNext();
        }
        
        if (Token.Type is not TokenType.CloseBracket) Report.Error($"The body for the type definition {identifierSpan.Text} should end with }}.", Token.Span);
        MoveNext(MoveInclude.NewLines);

        typeDefinition.TypeParameterList = genericsListDefinitionSyntax;
        return typeDefinition;
    }
    
    private FunctionCallSyntax ParseFunctionCall()
    {
        AssertToken([TokenType.Identifier, TokenType.BuiltInType], "Function call is missing a name.");
        var functionCall = new FunctionCallSyntax(new IdentifierSyntax(Token.Span));
        
        MoveNext();

        // if the function call has generics
        if (Token.Type is TokenType.LessThan)
        {
            functionCall.TypeList = ParseGenericsList();
            MoveNext();
        }
        
        AssertToken(TokenType.OpenParenthesis, "Function call is missing (.");
        MoveNext();
        
        if (Token.Type is not TokenType.CloseParenthesis)
        {
            while (true)
            {
                var argument = ParseExpression();
                MoveNext();
                
                functionCall.Arguments.Add(argument);

                if (Token.Type is TokenType.CloseParenthesis) break;
                AssertToken(TokenType.Comma, "Expected comma between function call parameters.");
                if (!MoveNext()) break;
            }
            
            AssertToken(TokenType.CloseParenthesis, "Function call is missing ).");
        }
        
        return functionCall;
    }
    
    private IndexorSyntax ParseIndexor()
    {
        AssertToken([TokenType.Identifier, TokenType.BuiltInType], "Indexor is missing a name.");
        var identifierSpan = Token.Span;
        MoveNext();
        
        AssertToken(TokenType.OpenSquareBracket, "Indexor is missing [.");
        MoveNext();
        
        UnsertToken(TokenType.CloseSquareBracket, "Indexors require an index to be specified.");
     
        var expression = ParseExpression();
        MoveNext();
        
        AssertToken(TokenType.CloseSquareBracket, "Indexor is missing ].");
        
        return new IndexorSyntax(new IdentifierSyntax(identifierSpan), expression);
    }
    
    private AccessorChainSyntax? ParseAccessorChain()
    {
        var accessor = new AccessorChainSyntax();

        while (true)
        {
            if (IsIndexor)
            {
                accessor.Chain.Add(ParseIndexor());
            }
            else if (IsFunctionCall)
            {
                accessor.Chain.Add(ParseFunctionCall());
            }
            else if (Token.Type is TokenType.Identifier)
            {
                accessor.Chain.Add(new IdentifierSyntax(Token.Span));
            }
            else
            {
                Report.Error("Expected indexor, identifier or function call.", Token.Span);
                return null;
            }

            // base case
            if (Peek()?.Type is not TokenType.DotAccessor)
            {
                break;
            }
        
            MoveNext();
            MoveNext();
        }
        
        return accessor;
    }

    private TypeParameterListSyntax ParseGenericsListDefinition()
    {
        var genericsListDefinitionSyntax = new TypeParameterListSyntax();
            
        MoveNext();

        if (Token.Type is TokenType.GreaterThan) Report.Error("Generics list definition should have at least one generic type.", Token.Span);
            
        while (true)
        {
            if (Token.Type is not TokenType.Identifier) Report.Error("Expected generic name.", Token.Span);
                
            genericsListDefinitionSyntax.Identifiers.Add(new IdentifierSyntax(Token.Span));
                
            MoveNext();
            if (Token.Type is TokenType.GreaterThan) break;
            if (Token.Type is not TokenType.Comma) Report.Error("Expected comma.", Token.Span);
            if (!MoveNext()) break;
        }
        
        if (Token.Type is not TokenType.GreaterThan) Report.Error("Generics list definition should end with >.", Token.Span);

        return genericsListDefinitionSyntax;
    }
    
    private TypeListSyntax ParseGenericsList()
    {
        var genericsListSyntax = new TypeListSyntax();
            
        MoveNext();

        if (Token.Type is TokenType.GreaterThan) Report.Error("Generics list should have at least one type.", Token.Span);
            
        while (true)
        {
            if (Token.Type is not TokenType.Identifier and not TokenType.BuiltInType) Report.Error("Expected type name.", Token.Span);
                
            genericsListSyntax.Types.Add(ParseType());
            MoveNext();
            
            if (Token.Type is TokenType.GreaterThan) break;
            if (Token.Type is not TokenType.Comma) Report.Error("Expected comma.", Token.Span);
            if (!MoveNext()) break;
        }
        
        if (Token.Type is not TokenType.GreaterThan) Report.Error("Generics list should end with >.", Token.Span);

        return genericsListSyntax;
    }

    private TypeSyntax ParseType(bool allowArrayType = true)
    {
        TypeSyntax? typeSyntax;
        TypeListSyntax? genericsListSyntax = null;
        
        if (Token.Type is not TokenType.Identifier and not TokenType.BuiltInType) Report.Error("Type should have a valid name.", Token.Span);

        var identifierSpan = Token.Span;
        var isBuiltIn = Token.Type is TokenType.BuiltInType;

        if (Peek()?.Type is TokenType.LessThan)
        {
            MoveNext();
            genericsListSyntax = ParseGenericsList();
        }

        if (Peek()?.Type is TokenType.OpenSquareBracket && allowArrayType)
        {
            MoveNext(MoveInclude.Whitespace | MoveInclude.NewLines);
            MoveNext(MoveInclude.Whitespace | MoveInclude.NewLines);
                    
            if (Token.Type is not TokenType.CloseSquareBracket) Report.Error("Expected ].", Token.Span);
                    
            typeSyntax = new ArrayTypeSyntax(identifierSpan);
        }
        else
        {
            typeSyntax = new TypeSyntax(identifierSpan);
        }
        
        typeSyntax.TypeList = genericsListSyntax;
        typeSyntax.IsBuiltIn = isBuiltIn;

        return typeSyntax;
    }
}