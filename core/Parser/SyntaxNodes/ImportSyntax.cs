using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class ImportSyntax(AccessorChainSyntax accessorChainChain) : Syntax
{
    public AccessorChainSyntax AccessorChainChain { get; } = accessorChainChain;
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        AccessorChainChain.Print();
        Printer.DecreasePadding();
    }
}