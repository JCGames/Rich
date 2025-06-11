namespace Fractals.Parser.SyntaxNodes;

public class ArrayInitializerSyntax(ExpressionSyntax expressionSyntax) : Syntax
{
    public ExpressionSyntax Expression { get; set; } = expressionSyntax;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        Expression.Print();
        Printer.DecreasePadding();
    }
}