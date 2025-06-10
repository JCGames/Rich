namespace Fractals.Parser.SyntaxNodes.Expressions;

public class BinaryOperatorSyntax : Syntax
{
    public Syntax? Left { get; set; }
    public Syntax? Right { get; set; }

    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Printer.PrintLine("Left:");
        Left?.Print();
        Printer.PrintLine("Right:");
        Right?.Print();
        
        Printer.DecreasePadding();
    }
}