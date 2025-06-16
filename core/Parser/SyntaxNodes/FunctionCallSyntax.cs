using Fractals.Lexer;
using Fractals.Parser.SyntaxNodes.Expressions;

namespace Fractals.Parser.SyntaxNodes;

public class FunctionCallSyntax(SpanMeta nameSpan) : Syntax, IAccessorChainLink
{
    public SpanMeta NameSpan { get; } = nameSpan;
    public GenericsListSyntax? Generics { get; set; }
    public List<ExpressionSyntax> Arguments { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {NameSpan.Text}");
        
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