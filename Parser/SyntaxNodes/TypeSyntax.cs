using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class TypeSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; set; } = span;
    public bool BuiltIn { get; set; }
    public GenericsListSyntax? Generics { get; set; }
    
    public override void Print()
    {
        Printer.PrintLine($"Type Name: {Span.Text}");
        Printer.IncreasePadding();
        Generics?.Print();
        Printer.DecreasePadding();
    }
}