namespace Fractals.Parser.SyntaxNodes;

public class IfSyntax(ExpressionSyntax condition) : LikeIfSyntax
{
    public ExpressionSyntax Condition { get; set; } = condition;
    public BlockSyntax Body { get; set; } = null!;
    public LikeIfSyntax? Branch { get; set; }
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Condition.Print();
        Body.Print();
        Branch?.Print();
        
        Printer.DecreasePadding();
    }
}