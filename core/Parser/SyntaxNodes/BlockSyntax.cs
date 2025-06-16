namespace Fractals.Parser.SyntaxNodes;

public enum BlockType
{
    Block,
    TopLevel
}

public class BlockSyntax(BlockType blockType) : Syntax
{
    public BlockType Type { get; set; } = blockType;
    public List<Syntax> Children { get; } = [];

    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: [");
        
        Printer.IncreasePadding();
        foreach (var syntax in Children)
        {
            syntax.Print();
        }
        Printer.DecreasePadding();
        
        Printer.PrintLine("]");
    }
}