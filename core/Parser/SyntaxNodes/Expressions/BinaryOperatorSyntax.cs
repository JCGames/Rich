using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes.Expressions;

public enum BinaryOperatorKind
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulus,
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqualTo,
    LessThanOrEqualTo,
    LogicalAnd,
    LogicalOr,
    Assignment
}

public class BinaryOperatorSyntax(SpanMeta operatorSpan, BinaryOperatorKind kind) : Syntax
{
    public SpanMeta OperatorSpan { get; init; } = operatorSpan;
    public BinaryOperatorKind Kind { get; } = kind;
    public Syntax? Left { get; init; }
    public Syntax? Right { get; init; }

    public override void Print()
    {
        PrintName();
        Printer.PrintLine($"Kind: {Kind}");
        
        Printer.PrintLine("Left:");
        Printer.IncreasePadding();
        Left?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Right:");
        Printer.IncreasePadding();
        Right?.Print();
        Printer.DecreasePadding();
    }
}