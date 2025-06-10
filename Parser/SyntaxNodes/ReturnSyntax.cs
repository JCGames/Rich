namespace Fractals.Parser.SyntaxNodes;

public class ReturnSyntax(ExpressionSyntax? expressionSyntax) : Syntax
{
    public ExpressionSyntax? Expression { get; } = expressionSyntax;

    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Expression?.Print();
        
        Printer.DecreasePadding();
    }
}