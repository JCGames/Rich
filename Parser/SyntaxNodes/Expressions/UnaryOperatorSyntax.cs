namespace Fractals.Parser.SyntaxNodes.Expressions;

public class UnaryOperatorSyntax : Syntax
{
    public Syntax? Operand { get; set; }
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        Printer.PrintLine("Operand:");
        Operand?.Print();
        Printer.DecreasePadding();
    }
}