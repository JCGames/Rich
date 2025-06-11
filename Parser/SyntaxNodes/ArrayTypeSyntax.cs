using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class ArrayTypeSyntax(SpanMeta span) : TypeSyntax(span)
{
    public override void Print()
    {
        Printer.PrintLine($"Array Type: {Span.Text}");
    }
}