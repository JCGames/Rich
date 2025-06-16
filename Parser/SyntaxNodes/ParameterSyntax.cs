using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class ParameterSyntax(SpanMeta nameSpan, TypeSyntax typeSyntax) : Syntax
{
    public SpanMeta NameSpan { get; } = nameSpan;
    public TypeSyntax Type { get; } = typeSyntax;
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {NameSpan.Text}");
        Printer.PrintLine("Type:");
        
        Printer.IncreasePadding();
        Type.Print();
        Printer.DecreasePadding();
    }
}