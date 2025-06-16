namespace Fractals.Parser.SyntaxNodes.Expressions;

public class BinaryOperatorSyntax : Syntax
{
    public Syntax? Left { get; init; }
    public Syntax? Right { get; init; }

    public override void Print()
    {
        PrintName();
        
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