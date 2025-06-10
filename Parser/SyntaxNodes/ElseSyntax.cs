namespace Fractals.Parser.SyntaxNodes;

public class ElseSyntax : LikeIfSyntax
{
    public BlockSyntax Body { get; set; } = null!;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Body.Print();
        
        Printer.DecreasePadding();
    }
}