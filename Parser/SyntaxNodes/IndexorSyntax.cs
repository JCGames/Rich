namespace Fractals.Parser.SyntaxNodes;

public class IndexorSyntax(IdentifierSyntax identifierSyntax, ExpressionSyntax expressionSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public ExpressionSyntax IndexExpression { get; } = expressionSyntax;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Identifier.Print();
        IndexExpression.Print();
        
        Printer.DecreasePadding();
    }
}