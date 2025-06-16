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

public class BinaryOperatorSyntax(BinaryOperatorKind kind) : Syntax
{
    public BinaryOperatorKind Kind { get; init; } = kind;
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