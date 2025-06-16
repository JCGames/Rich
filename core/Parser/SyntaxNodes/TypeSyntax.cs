using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class TypeSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; } = span;
    public bool BuiltIn { get; set; }
    public GenericsListSyntax? Generics { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {Span.Text}");
        
        Printer.PrintLine("Generics:");
        Printer.IncreasePadding();
        Generics?.Print();
        Printer.DecreasePadding();
    }
}