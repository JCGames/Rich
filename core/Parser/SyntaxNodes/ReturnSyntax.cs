namespace Rich.Parser.SyntaxNodes;

public class ReturnSyntax(ExpressionSyntax? expressionSyntax) : Syntax
{
    public ExpressionSyntax? Expression { get; } = expressionSyntax;

    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        Expression?.Print();
        Printer.DecreasePadding();
    }
}