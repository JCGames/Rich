using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class IndexorSyntax(IdentifierSyntax identifierSyntax, ExpressionSyntax expressionSyntax) : Syntax, IAccessorChainLink
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public ExpressionSyntax IndexExpression { get; } = expressionSyntax;
    
    public override void Print()
    {
        PrintName();
        
        Identifier.Print();
        
        IndexExpression.Print();
    }

    public bool IsIdentifier => false;
    public bool IsAccessor => false;
    public bool IsFunctionCall => false;
    public bool IsIndexor => true;
}