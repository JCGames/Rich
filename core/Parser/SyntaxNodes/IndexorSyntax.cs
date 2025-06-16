namespace Fractals.Parser.SyntaxNodes;

public class IndexorSyntax(IdentifierSyntax identifierSyntax, ExpressionSyntax expressionSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public ExpressionSyntax IndexExpression { get; } = expressionSyntax;
    
    public override void Print()
    {
        PrintName();
        
        Identifier.Print();
        
        IndexExpression.Print();
    }
}