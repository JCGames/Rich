using Rich.Lexer;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class FunctionCallSyntax(IdentifierSyntax identifierSyntax) : Syntax, IAccessorChainLink
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public GenericsListSyntax? Generics { get; set; }
    public List<ExpressionSyntax> Arguments { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {identifierSyntax.Span.Text}");
        
        Generics?.Print();
        
        Printer.PrintLine("Arguments: [");
        Printer.IncreasePadding();
        foreach (var argument in Arguments)
        {
            argument.Print();
        }
        Printer.DecreasePadding();
        Printer.PrintLine("]");
    }

    public bool IsIdentifier => false;
    public bool IsAccessor => false;
    public bool IsFunctionCall => true;
    public bool IsIndexor => false;
}