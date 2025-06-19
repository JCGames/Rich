namespace Rich.Parser.SyntaxNodes;

public class NewSyntax(AccessorChainSyntax accessorChain) : Syntax
{
    public AccessorChainSyntax AccessorChain { get; } = accessorChain;
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        AccessorChain.Print();
        Printer.DecreasePadding();
    }
}