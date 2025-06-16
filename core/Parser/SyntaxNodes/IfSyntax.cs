namespace Rich.Parser.SyntaxNodes;

public class IfSyntax(ExpressionSyntax condition) : LikeIfSyntax
{
    public ExpressionSyntax Condition { get; } = condition;
    public BlockSyntax Body { get; init; } = null!;
    public LikeIfSyntax? Branch { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine("Condition:");
        Printer.IncreasePadding();
        Condition.Print();
        Printer.DecreasePadding();
        
        Body.Print();
        
        Printer.PrintLine("If Branch:");
        Printer.IncreasePadding();
        Branch?.Print();
        Printer.DecreasePadding();
    }
}