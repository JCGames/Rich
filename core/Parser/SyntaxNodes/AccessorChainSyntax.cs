namespace Rich.Parser.SyntaxNodes;

public class AccessorChainSyntax : Syntax
{
    public List<Syntax> Chain { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine("Chain: [");
        Printer.IncreasePadding();
        for (var i = 0; i < Chain.Count; i++)
        {
            Chain[i].Print();
            
            if (i != Chain.Count - 1)
            {
                Printer.PrintLine(".");
            }
        }
        Printer.DecreasePadding();
        Printer.PrintLine("]");
    }
}