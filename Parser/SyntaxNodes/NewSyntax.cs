namespace Fractals.Parser.SyntaxNodes;

public class NewSyntax(Syntax accessorChain) : Syntax
{
    public Syntax AccessorChain { get; set; } = accessorChain;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        AccessorChain.Print();
        
        Printer.DecreasePadding();
    }
}