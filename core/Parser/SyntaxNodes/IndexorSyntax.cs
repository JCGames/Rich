using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class IndexorSyntax(IdentifierSyntax identifierSyntax, ExpressionSyntax expressionSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public ExpressionSyntax IndexExpression { get; } = expressionSyntax;
    public VariableDeclarationSyntax? Binding { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Identifier.Print();
        
        Printer.PrintLine("Index:");
        Printer.IncreasePadding();
        IndexExpression.Print();
        Printer.DecreasePadding();
    }
}