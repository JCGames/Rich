namespace Fractals.Parser.SyntaxNodes;

public class WhileSyntax(ExpressionSyntax condition) : Syntax
{
    public ExpressionSyntax Condition { get; set; } = condition;
    public BlockSyntax Body { get; set; } = null!;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Condition.Print();
        Body.Print();
        
        Printer.DecreasePadding();
    }
}