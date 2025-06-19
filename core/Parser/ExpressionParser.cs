using Microsoft.VisualBasic;
using Rich.Diagnostics;
using Rich.Lexer;
using Rich.Parser.SyntaxNodes;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser;

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
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.LogicalAnd)
                {
                    Left = left,
                    Right = ParseEquality()
                };
            }
            else if (Token.Type is TokenType.LogicalOr)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.LogicalOr)
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
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Equals)
                {
                    Left = left,
                    Right = ParseRelational()
                };
            }
            else if (Token.Type is TokenType.NotEquals)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.NotEquals)
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
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.GreaterThan)
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.LessThan)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.LessThan)
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.GreaterThanOrEqual)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.GreaterThanOrEqualTo)
                {
                    Left = left,
                    Right = ParseAdditive()
                };
            }
            else if (Token.Type is TokenType.LessThanOrEqual)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.LessThanOrEqualTo)
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
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Addition)
                {
                    Left = left,
                    Right = ParseMultiplicative()
                };
            }
            else if (Token.Type is TokenType.Subtraction)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Subtraction)
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
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Multiplication)
                {
                    Left = left,
                    Right = ParsePrimary()
                };
            }
            else if (Token.Type is TokenType.Division)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Division)
                {
                    Left = left,
                    Right = ParsePrimary()
                };
            }
            else if (Token.Type is TokenType.Modulus)
            {
                var tokenSpan = Token.Span;
                MoveNext();
                left = new BinaryOperatorSyntax(tokenSpan, BinaryOperatorKind.Modulus)
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
            
            var accessorChain = ParseAccessorChain();
            MoveNext();
            
            switch (accessorChain?.Chain.FirstOrDefault())
            {
                case FunctionCallSyntax or IndexorSyntax:
                    return new NewSyntax(accessorChain);
                default:
                    Report.Error("Invalid new operation.", Token.Span);
                    return null;
            }
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
                return new UnaryOperatorSyntax(Token.Span, UnaryOperatorKind.Negation) { Operand = ParsePrimary() };
            case TokenType.Not:
                MoveNext();
                return new UnaryOperatorSyntax(Token.Span, UnaryOperatorKind.Not) { Operand = ParsePrimary() };
            case TokenType.OpenParenthesis:
            {
                MoveNext();
                result = ParseExpression();
                MoveNext();
                
                if (Token.Type is not TokenType.CloseParenthesis) Report.Error("Expected a ).", Token.Span);
            }
                break;
        }
        
        if (result is null) Report.Error("Invalid term in expression.", Token.Span);
        
        MoveNext();
        return result;
    }
}