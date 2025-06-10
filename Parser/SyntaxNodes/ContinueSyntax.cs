namespace Fractals.Parser.SyntaxNodes;

public class ContinueSyntax : Syntax
{
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
    }
}