using Rich.Lexer;
using Rich.Semantics;

namespace Rich.Parser.SyntaxNodes;

public class TypeSyntax(SpanMeta span) : Syntax
{
    public bool IsBuiltIn { get; set; }
    public SpanMeta Span { get; } = span;
    public TypeListSyntax? TypeList { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {Span.Text}");
        Printer.PrintLine($"Built-in: {IsBuiltIn}");
        
        Printer.PrintLine("Generics:");
        Printer.IncreasePadding();
        TypeList?.Print();
        Printer.DecreasePadding();
    }
}