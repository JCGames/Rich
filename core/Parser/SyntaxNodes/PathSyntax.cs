using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class PathSyntax(IAccessorChainLink accessorChain) : Syntax
{
    public IAccessorChainLink AccessorChain { get; } = accessorChain;
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        AccessorChain.Print();
        Printer.DecreasePadding();
    }
}