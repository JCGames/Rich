namespace Fractals.Parser.SyntaxNodes;

public abstract class Syntax
{
    protected void PrintName()
    {
        var normalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Printer.PrintLine(GetType().Name);
        Console.ForegroundColor = normalColor;
    }
    
    public abstract void Print();
}