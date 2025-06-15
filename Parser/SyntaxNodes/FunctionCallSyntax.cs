using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class FunctionCallSyntax(SpanMeta nameSpan) : Syntax
{
    public SpanMeta NameSpan { get; } = nameSpan;
    public List<ExpressionSyntax> Arguments { get; } = [];
    public GenericsListSyntax? Generics { get; set; }
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.PrintLine($"Name: {NameSpan.Text}");
        Printer.PrintLine("Arguments: [");
        Printer.IncreasePadding();

        foreach (var argument in Arguments)
        {
            argument.Print();
        }
        
        Printer.DecreasePadding();
        Printer.PrintLine("]");
        Generics?.Print();
    }
}