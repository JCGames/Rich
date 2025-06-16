namespace Rich.Parser.SyntaxNodes;

public class WhileSyntax(ExpressionSyntax condition) : Syntax
{
    public ExpressionSyntax Condition { get; } = condition;
    public BlockSyntax Body { get; init; } = null!;
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine("Condition:");
        Printer.IncreasePadding();
        Condition.Print();
        Printer.DecreasePadding();
        
        Body.Print();
    }
}