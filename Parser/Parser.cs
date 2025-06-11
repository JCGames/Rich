using Fractals.Diagnostics;
using Fractals.Lexer;
using Fractals.Parser.SyntaxNodes;
using Fractals.Parser.SyntaxNodes.Expressions;

namespace Fractals.Parser;

public partial class Parser
{
    private List<Token> _tokens = null!;
    private int _current;
    private Token Token => _tokens[_current];
    private bool IsEndOfTokens => Token.Type is TokenType.EndOfFile;
    private bool IsFunctionCall => Token.Type is TokenType.Identifier && Peek()?.Type is TokenType.OpenParenthesis;
    private bool IsEndOfStatement => Token.Type is TokenType.EndOfLine or TokenType.EndOfFile or TokenType.CloseBracket;
    private bool IsIndexor => Token.Type is TokenType.Identifier && Peek()?.Type is TokenType.OpenSquareBracket;
    
    /// <summary>
    /// By default, <see cref="MoveNext"/> ignores new lines and whitespace.
    /// To stop on whitespace or new lines or both, pass a <see cref="Sensitivity"/>.
    /// </summary>
    /// <param name="sensitivity">What should <see cref="MoveNext"/> stop on.</param>
    /// <returns></returns>
    private bool MoveNext(Sensitivity sensitivity = Sensitivity.None)
    {
        if (_current + 1 >= _tokens.Count) return false;

        _current++;

        Func<bool> predicate = sensitivity switch
        {
            Sensitivity.None => () => 
                Token.Type is TokenType.Whitespace or TokenType.EndOfLine,
            Sensitivity.NewLines => () => Token.Type is TokenType.Whitespace,
            Sensitivity.Whitespace => () => Token.Type is TokenType.EndOfLine,
            Sensitivity.Whitespace | Sensitivity.NewLines => () => false,
            _ => () =>
                Token.Type is TokenType.Whitespace or TokenType.EndOfLine
        };

        while (_current < _tokens.Count - 1 && predicate())
        {
            _current++;
        }
        
        return true;
    }

    /// <summary>
    /// Will only move to the next token if the current token is an end of line token.
    /// </summary>
    private void MoveNextIfEndOfLine()
    {
        if (Token.Type is TokenType.EndOfLine) MoveNext();
    }
    
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
    
    public SyntaxTree Run(List<Token> tokens)
    {
        _tokens = tokens;
        return new SyntaxTree
        {
            FilePath = _tokens[0].Span.FilePath,
            Root = ParseBlock(BlockType.TopLevel)
        };
    }

    /// <summary>
    /// Parses a block.
    /// </summary>
    /// <param name="blockType">When set to <see cref="BlockType.Block"/> the last token moved to will be a new line or end of file.</param>
    /// <returns></returns>
    private BlockSyntax ParseBlock(BlockType blockType)
    {
        var blockSyntax = new BlockSyntax(blockType);
        
        switch (blockType)
        {
            case BlockType.Block:
                if (Token.Type is not TokenType.OpenBracket) Diagnoser.AddError("Expected open bracket.", Token.Span);
                MoveNext();
                
                while (!IsEndOfTokens && Token.Type is not TokenType.CloseBracket)
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
                
                if (Token.Type is not TokenType.CloseBracket) Diagnoser.AddError("Expected close bracket.", Token.Span);
                MoveNext(Sensitivity.NewLines);
                break;
            case BlockType.Namespace:
                Diagnoser.AddError("Not implemented yet.", Token.Span);
                break;
            case BlockType.TopLevel:
                while (!IsEndOfTokens)
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
                break;
        }
        
        return blockSyntax;
    }
    
    private Syntax? ParseStatement()
    {
        if (Token.Type is TokenType.KeywordType)
        {
            var typeDefinition = ParseTypeDefinition();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return typeDefinition;
        }
        
        if (Token.Type is TokenType.KeywordFunction)
        {
            var function = ParseFunction();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return function;
        }

        if (Token.Type is TokenType.KeywordReturn)
        {
            var @return = ParseReturn();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return @return;
        }

        if (Token.Type is TokenType.KeywordBreak)
        {
            var @break = ParseBreak();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return @break;
        }
        
        if (Token.Type is TokenType.KeywordContinue)
        {
            var @continue = ParseContinue();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return @continue;
        }
        
        if (Token.Type is TokenType.KeywordWhile)
        {
            var @while = ParseWhile();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return @while;
        }
        
        if (Token.Type is TokenType.KeywordIf)
        {
            var @if = ParseIf();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return @if;
        }
        
        if (Token.Type is TokenType.KeywordImport)
        {
            var import = ParseImport();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return import;
        }
        
        if (Token.Type is TokenType.Identifier)
        {
            var statement = ParseStatementThatStartsWithIdentifier();
            if (!IsEndOfStatement) Diagnoser.AddError($"Expected the end of a statement but got {Token.Span.Text}.", Token.Span);
            MoveNextIfEndOfLine();
            return statement;
        }

        if (Token.Type is TokenType.BuiltInType) Diagnoser.AddError("Type should not be here.", Token.Span);
        if (Token.Type is TokenType.Whitespace or TokenType.EndOfLine) return null;
        
        Diagnoser.AddError($"Unknown statement: {Token.Span.Text}.", Token.Span);
        return null;
    }

    /// <summary>
    /// Lands on the close ) parentheses of the function call.
    /// </summary>
    /// <returns></returns>
    private FunctionCallSyntax ParseFunctionCall()
    {
        // myFunc
        if (Token.Type is not TokenType.Identifier) Diagnoser.AddError("Expected identifier.", Token.Span);

        var functionCall = new FunctionCallSyntax(Token.Span);
        MoveNext();
        
        // myFunc (
        if (Token.Type is not TokenType.OpenParenthesis) Diagnoser.AddError("Expected open parenthesis.", Token.Span);
        MoveNext();

        // myFunc (expression ?
        if (Token.Type is not TokenType.CloseParenthesis)
        {
            while (true)
            {
                var argument = ParseExpression();
                MoveNext();
                
                functionCall.Arguments.Add(argument);

                if (Token.Type is TokenType.CloseParenthesis) break;
                if (Token.Type is not TokenType.Comma) Diagnoser.AddError("Expected comma.", Token.Span);
                if (!MoveNext()) break;
            }
            
            if (Token.Type is not TokenType.CloseParenthesis) Diagnoser.AddError("Expected close parenthesis.", Token.Span);
        }
        // otherwise myFunc ()
        
        return functionCall;
    }

    /// <code>
    /// KeywordFunction Identifier OpenParenthesis (Identifier Colon [BuiltInType | Identifier] Comma)... CloseParenthesis
    /// OpenBracket CloseBracket EndOfLine
    /// </code>
    private FunctionSyntax ParseFunction()
    {
        if (Token.Type is not TokenType.KeywordFunction) Diagnoser.AddError("Expected function keyword.", Token.Span);
        MoveNext();
        
        if (Token.Type is not TokenType.Identifier) Diagnoser.AddError("Expected identifier.", Token.Span);
        
        var function = new FunctionSyntax(Token.Span);
        
        MoveNext();
        
        if (Token.Type is not TokenType.OpenParenthesis) Diagnoser.AddError("Expected open parenthesis.", Token.Span);
        MoveNext();

        if (Token.Type is not TokenType.CloseParenthesis)
        {
            while (true)
            {
                if (Token.Type is not TokenType.Identifier) Diagnoser.AddError("Expected identifier.", Token.Span);
                var parameterNameSpan = Token.Span;
                MoveNext();
                
                if (Token.Type is not TokenType.Colon) Diagnoser.AddError("Expected colon.", Token.Span);
                MoveNext();
                
                if (Token.Type is not TokenType.Identifier and not TokenType.BuiltInType) Diagnoser.AddError("Expected built-in type or identifier.", Token.Span);
                var typeNameSpan = Token.Span;
                var isBuiltIn = Token.Type is TokenType.BuiltInType;
                MoveNext();
                
                var parameter = new ParameterSyntax(parameterNameSpan, new TypeSyntax(typeNameSpan) { BuiltIn = isBuiltIn });
                function.Parameters.Add(parameter);

                if (Token.Type is TokenType.CloseParenthesis) break;
                if (Token.Type is not TokenType.Comma) Diagnoser.AddError("Expected comma.", Token.Span);
                if (!MoveNext()) break;
            }
            
            if (Token.Type is not TokenType.CloseParenthesis) Diagnoser.AddError("Expected close parenthesis.", Token.Span);
        }

        MoveNext();

        if (Token.Type is TokenType.Colon)
        {
            MoveNext();
            
            if (Token.Type is not TokenType.BuiltInType and not TokenType.Identifier) Diagnoser.AddError("Expected built-in type or identifier.", Token.Span);
            var typeNameSpan = Token.Span;
            var isBuiltIn = Token.Type is TokenType.BuiltInType;
            MoveNext();

            var type = new TypeSyntax(typeNameSpan) { BuiltIn = isBuiltIn };
            
            function.ReturnType = type;
        }
        
        if (Token.Type is not TokenType.OpenBracket) Diagnoser.AddError("Expected open bracket.", Token.Span);
        function.Body = ParseBlock(BlockType.Block);
        
        return function;
    }
    
    /// <code>
    /// AccessorChain Colon [BuiltInType | Identifier] (Assignment Expression?)? EndOfLine
    /// </code>
    private Syntax? ParseStatementThatStartsWithIdentifier(bool acceptAnyAccessorChain = false)
    {
        var accessorChain = ParseAccessorChain();
        
        if (Peek()?.Type is TokenType.Colon)
        {
            MoveNext();

            // if what was returned from the ParseAccessorTree method
            // was not a IdentifierSyntax then we don't have a variable
            // declaration, and we should quickly exit with an error
            if (accessorChain is not IdentifierSyntax accessorChainForReal)
            {
                Diagnoser.AddError("Expected identifier.", Token.Span);
                return null;
            }
            
            // move to what could be a type or an assignment
            // if the next token is an assignment, the variable is an auto property
            // if the next token is an identifier or built-in type,
            //      the variable could be a declaration without an expression or with one
            // if the variable declaration does not have an expression it is expected
            // to obtain a default value or be assigned a value
            MoveNext();

            TypeSyntax? typeSyntax = null;
            
            if (Token.Type is TokenType.BuiltInType or TokenType.Identifier)
            {
                var identifierSpan = Token.Span;
                
                if (Peek()?.Type is TokenType.OpenSquareBracket)
                {
                    MoveNext();
                    MoveNext(Sensitivity.Whitespace | Sensitivity.NewLines);
                    
                    if (Token.Type is not TokenType.CloseSquareBracket) Diagnoser.AddError("Expected close bracket.", Token.Span);
                    
                    typeSyntax = new ArrayTypeSyntax(identifierSpan);
                }
                else
                {
                    typeSyntax = new TypeSyntax(identifierSpan);
                }

                if (Peek()?.Type is TokenType.Assignment)
                {
                    MoveNext();
                }
                else
                {
                    MoveNext(Sensitivity.NewLines);
                    return new VariableDeclarationSyntax(
                        accessorChainForReal,
                        typeSyntax,
                        null);
                }
            }
            else if (Token.Type is not TokenType.Assignment)
            {
                Diagnoser.AddError("Invalid variable declaration.", Token.Span);
            }

            if (Token.Type is TokenType.Assignment)
            {
                MoveNext();
                var declaration = new VariableDeclarationSyntax(
                    accessorChainForReal,
                    typeSyntax,
                    ParseExpression());
                
                MoveNext(Sensitivity.NewLines);
                return declaration;
            }
        }
        else if (Peek()?.Type is TokenType.Assignment)
        {
            MoveNext();
            MoveNext();
            var expression = ParseExpression();
            MoveNext(Sensitivity.NewLines);
            return new AssignmentSyntax
            {
                Left = accessorChain,
                Right = expression
            };
        }
        else if (IsRightMostAFunctionCall() || acceptAnyAccessorChain)
        {
            MoveNext(Sensitivity.NewLines);
            return accessorChain;
        }
            
        Diagnoser.AddError("Incomplete statement.", Token.Span);
        return null;

        bool IsRightMostAFunctionCall()
        {
            if (accessorChain is AccessorSyntax accessor)
            {
                while (accessor.Right is AccessorSyntax right)
                {
                    accessor = right;
                }
                
                return accessor.Right is FunctionCallSyntax;
            }
            return accessorChain is FunctionCallSyntax;
        }
    }

    /// <summary>
    /// Lands on the close ] bracket when this method exits.
    /// </summary>
    /// <returns></returns>
    private IndexorSyntax ParseIndexor()
    {
        var identifierSpan = Token.Span;
        MoveNext();
        MoveNext();

        if (Token.Type is TokenType.CloseSquareBracket) Diagnoser.AddError("No expression given to the indexor.", Token.Span);
     
        var expression = ParseExpression();
        MoveNext();
        
        if (Token.Type is not TokenType.CloseSquareBracket) Diagnoser.AddError("Expected close square bracket.", Token.Span);
        
        return new IndexorSyntax(new IdentifierSyntax(identifierSpan), expression);
    }

    /// <summary>
    /// Should land on the identifier or close ] bracket of an indexor.
    /// </summary>
    /// <returns></returns>
    private Syntax? ParseAccessorChain()
    {
        Syntax? left;
        
        if (IsIndexor)
        {
            left = ParseIndexor();
        }
        else if (IsFunctionCall)
        {
            left = ParseFunctionCall();
        }
        else if (Token.Type is TokenType.Identifier)
        {
            left = new IdentifierSyntax(Token.Span);
        }
        else
        {
            left = null;
            Diagnoser.AddError("Expected indexor or identifier.", Token.Span);
        }

        if (Peek()?.Type is not TokenType.DotAccessor)
        {
            return left;
        }
        
        MoveNext();
        MoveNext();

        var right = ParseAccessorChain();
        
        return new AccessorSyntax
        {
            Left = left,
            Right = right
        };
    }
    
    private ReturnSyntax ParseReturn()
    {
        MoveNext();
        var @return = new ReturnSyntax(ParseExpression());
        MoveNext(Sensitivity.NewLines);
        return @return;
    }

    private BreakSyntax ParseBreak()
    {
        MoveNext(Sensitivity.NewLines);
        return new BreakSyntax();
    }
    
    private ContinueSyntax ParseContinue()
    {
        MoveNext(Sensitivity.NewLines);
        return new ContinueSyntax();
    }

    private WhileSyntax ParseWhile()
    {
        MoveNext();
        var condition = ParseExpression();
        MoveNext();
        
        if (Token.Type is not TokenType.OpenBracket) Diagnoser.AddError("Expected open bracket.", Token.Span);

        var body = ParseBlock(BlockType.Block);
        
        return new WhileSyntax(condition)
        {
            Body = body
        };
    }

    private IfSyntax ParseIf()
    {
        MoveNext();
        var condition = ParseExpression();
        MoveNext();
        
        if (Token.Type is not TokenType.OpenBracket) Diagnoser.AddError("Expected open bracket.", Token.Span);
        
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
                Diagnoser.AddError("Invalid if conditional branch.", Token.Span);
            }
        }

        return @if;
    }

    private ImportSyntax ParseImport()
    {
        MoveNext();
        if (Token.Type is not TokenType.StringLiteral) Diagnoser.AddError("Expected string literal.", Token.Span);
        var stringLiteralSpan = Token.Span;
        MoveNext(Sensitivity.NewLines);
        return new ImportSyntax(stringLiteralSpan);
    }

    private TypeDefinitionSyntax ParseTypeDefinition()
    {
        MoveNext();
        
        if (Token.Type is not TokenType.Identifier) Diagnoser.AddError("Expected identifier.", Token.Span);
        var identifierSpan = Token.Span;

        MoveNext();

        if (Token.Type is not TokenType.OpenBracket) Diagnoser.AddError("Expected open bracket.", Token.Span);

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
                    Diagnoser.AddError("Expected variable declaration.", Token.Span);
                }
            }
            else if (Token.Type is TokenType.KeywordFunction)
            {
                typeDefinition.Functions.Add(ParseFunction());
            }
            else
            {
                Diagnoser.AddError("Expected function or variable.", Token.Span);
            }
            MoveNext();
        }
        
        if (Token.Type is not TokenType.CloseBracket) Diagnoser.AddError("Expected closing bracket.", Token.Span);
        MoveNext(Sensitivity.NewLines);

        return typeDefinition;
    }
}