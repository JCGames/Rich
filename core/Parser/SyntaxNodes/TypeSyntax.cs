using Rich.Lexer;
using Rich.Semantics;

namespace Rich.Parser.SyntaxNodes;

public class TypeSyntax(SpanMeta span) : Syntax
{
    public bool IsBuiltIn { get; set; }
    public SpanMeta Span { get; } = span;
    public GenericsListSyntax? GenericsList { get; set; }
    public TypeBinding? TypeBinding { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {Span.Text}");
        Printer.PrintLine($"Built-in: {IsBuiltIn}");
        
        Printer.PrintLine("Generics:");
        Printer.IncreasePadding();
        GenericsList?.Print();
        Printer.DecreasePadding();
    }
}