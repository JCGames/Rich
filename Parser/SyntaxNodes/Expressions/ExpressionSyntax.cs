namespace Fractals.Parser.SyntaxNodes;

public class ExpressionSyntax(Syntax? root) : Syntax
{
    public Syntax? Root { get; set; } = root;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Root?.Print();
        
        Printer.DecreasePadding();
    }
}