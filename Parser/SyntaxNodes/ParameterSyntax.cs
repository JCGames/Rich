using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class ParameterSyntax(SpanMeta nameSpan, TypeSyntax typeSyntax) : Syntax
{
    public SpanMeta NameSpan { get; set; } = nameSpan;
    public TypeSyntax Type { get; set; } = typeSyntax;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Printer.PrintLine($"Parameter Name: {NameSpan.Text}");
        Type.Print();
        
        Printer.DecreasePadding();
    }
}