using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;

public class IntegerSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; } = span;
     
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }
}