namespace Fractals.Parser.SyntaxNodes;

public class BreakSyntax : Syntax
{
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
    }
}