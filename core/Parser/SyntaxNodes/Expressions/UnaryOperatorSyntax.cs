namespace Fractals.Parser.SyntaxNodes.Expressions;

public class UnaryOperatorSyntax : Syntax
{
    public Syntax? Operand { get; init; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        Operand?.Print();
        Printer.DecreasePadding();
    }
}