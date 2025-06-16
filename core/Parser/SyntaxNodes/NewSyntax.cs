namespace Fractals.Parser.SyntaxNodes;

public class NewSyntax(Syntax accessorChain) : Syntax
{
    public Syntax AccessorChain { get; } = accessorChain;
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        AccessorChain.Print();
        Printer.DecreasePadding();
    }
}