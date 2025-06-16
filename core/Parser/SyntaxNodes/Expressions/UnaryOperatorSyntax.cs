namespace Rich.Parser.SyntaxNodes.Expressions;

public enum UnaryOperatorKind
{
    Negation,
    Not
}

public class UnaryOperatorSyntax(UnaryOperatorKind kind) : Syntax
{
    public UnaryOperatorKind Kind { get; init; } = kind;
    public Syntax? Operand { get; init; }
    
    public override void Print()
    {
        PrintName();
        Printer.PrintLine($"Kind: {Kind}");
        
        Printer.IncreasePadding();
        Operand?.Print();
        Printer.DecreasePadding();
    }
}