using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class FunctionCallSyntax(SpanMeta nameSpan) : Syntax
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
}