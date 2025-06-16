using Fractals.Diagnostics;
using Fractals.Lexer;
using Fractals.Parser.SyntaxNodes;
using Fractals.Parser.SyntaxNodes.Expressions;
using Microsoft.VisualBasic;

namespace Fractals.Parser;

public partial class Parser
{
    /// <summary>
    /// Parse expression will parse a multi-line expression and end on the last token of that expression.
    /// It is up to the developer to move the position forward for the next statement.
    /// </summary>
    /// <returns></returns>
    private ExpressionSyntax ParseExpression()
    {
        var expression = new ExpressionSyntax(ParseLogical());
        
        // artificially back up to the last token
        if (_current - 1 >= 0)
        {
            _current--;
        
            while (_current - 1 >= 0 && _tokens[_current].Type is TokenType.Whitespace or TokenType.EndOfLine)
            {
                _current--;
            }
        }
        
        return expression;
    }
    
    private Syntax? ParseLogical()
    {
        var left = ParseEquality();

        while (Token.Type is TokenType.LogicalAnd or TokenType.LogicalOr)
        {
            if (Token.Type is TokenType.LogicalAnd)
            {
                MoveNext();
                left = new LogicalAndSyntax
                {
                    Left = left,
                    Right = ParseEquality()
                };
            }
            else if (Token.Type is TokenType.LogicalOr)
            {
                MoveNext();
                left = new LogicalOrSyntax
                {
                    Left = left,
                    Right = ParseEquality()
                };
            }
        }

        return left;
    }

    private Syntax? ParseEquality()
    {
        var left = ParseRelational();

        while (Token.Type is TokenType.Equals or TokenType.NotEquals)
        {
            if (Token.Type is TokenType.Equals)
            {
                MoveNext();
                left = new EqualsSyntax
                {
                    Left = left,
                    Right = ParseRelational()
                };
            }
            else if (Token.Type is TokenType.NotEquals)
            {
                MoveNext();
                left = new NotEqualsSyntax
                {
                    Left = left,
                    Right = ParseRelational()
                };
            }
        }

        return left;
    }
    
    private Syntax? ParseRelational()
    {
        var left = ParseAdditive();

        while (Token.Type is TokenType.GreaterThan
               or TokenType.LessThan
               or TokenType.GreaterThanOrEqual
               or TokenType.LessThanOrEqual)
        {
            if (Token.Type is TokenType.GreaterThan)
            {
                MoveNext();
                left = new GreaterThanSyntax
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.LessThan)
            {
                MoveNext();
                left = new LessThanSyntax
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.GreaterThanOrEqual)
            {
                MoveNext();
                left = new GreaterThanOrEqualSyntax
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.LessThanOrEqual)
            {
                MoveNext();
                left = new LessThanOrEqualSyntax
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
        }

        return left;
    }

    private Syntax? ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (Token.Type is TokenType.Addition or TokenType.Subtraction)
        {
            if (Token.Type is TokenType.Addition)
            {
                MoveNext();
                left = new AdditionSyntax
                {
                    Left = left,
                    Right = ParseMultiplicative()
                };
            }
            else if (Token.Type is TokenType.Subtraction)
            {
                MoveNext();
                left = new SubtractionSyntax
                {
                    Left = left,
                    Right = ParseMultiplicative()
                };
            }
        }

        return left;
    }

    private Syntax? ParseMultiplicative()
    {
        var left = ParsePrimary();

        while (Token.Type is TokenType.Multiplication or TokenType.Division or TokenType.Modulus)
        {
            if (Token.Type is TokenType.Multiplication)
            {
                MoveNext();
                left = new MultiplicationSyntax
                {
                    Left = left,
                    Right = ParsePrimary()
                };
            }
            else if (Token.Type is TokenType.Division)
            {
                MoveNext();
                left = new DivisionSyntax
                {
                    Left = left,
                    Right = ParsePrimary()
                };
            }
            else if (Token.Type is TokenType.Modulus)
            {
                MoveNext();
                left = new ModulusSyntax
                {
                    Left = left,
                    Right = ParsePrimary()
                };
            }
        }

        return left;
    }

    private Syntax? ParsePrimary()
    {
        Syntax? result = null;
        
        if (IsIndexor || IsFunctionCall || Token.Type is TokenType.Identifier)
        {
            var accessor = ParseAccessorChain();
            MoveNext();
            return accessor;
        }

        if (Token.Type is TokenType.KeywordNew)
        {
            MoveNext();
            
            if (Token.Type is TokenType.OpenSquareBracket)
            {
                MoveNext();
                var expression = ParseExpression();
                MoveNext();
            
                if (Token.Type is not TokenType.CloseSquareBracket) Diagnoser.AddError("Expected a ).", Token.Span);

                MoveNext();
            
                return new ArrayInitializerSyntax(expression);
            }
            
            var fc = ParseStatementThatStartsWithIdentifier(acceptAnyAccessorChain: true);
            MoveNextIfEndOfLine();

            if (fc is FunctionCallSyntax functionCall)
            {
                return new NewSyntax(functionCall);
            }
            
            if (fc is AccessorSyntax { Left: FunctionCallSyntax } accessor)
            {
                return new NewSyntax(accessor);
            }
            
            Diagnoser.AddError("Invalid new operation.", Token.Span);
            return null;
        }
        
        switch (Token.Type)
        {
            case TokenType.Integer:
                result = new IntegerSyntax(Token.Span);
                break;
            case TokenType.Decimal:
                result = new DecimalSyntax(Token.Span);
                break;
            case TokenType.Boolean:
                result = new BooleanSyntax(Token.Span);
                break;
            case TokenType.StringLiteral:
                result = new StringLiteralSyntax(Token.Span);
                break;
            case TokenType.BuiltInType:
                result = new IdentifierSyntax(Token.Span);
                break;
            case TokenType.Nothing:
                result = new NothingSyntax();
                break;
            case TokenType.Subtraction:
                MoveNext();
                return new NegationSyntax { Operand = ParsePrimary() };
            case TokenType.OpenParenthesis:
            {
                MoveNext();
                result = ParseExpression();
                MoveNext();
                
                if (Token.Type is not TokenType.CloseParenthesis) Diagnoser.AddError("Expected a ).", Token.Span);
            }
                break;
        }
        
        if (result is null) Diagnoser.AddError("Invalid term in expression.", Token.Span);
        
        MoveNext();
        return result;
    }
}