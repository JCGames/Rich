using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class StringLiteralSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; init; } = span;
    
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }
}