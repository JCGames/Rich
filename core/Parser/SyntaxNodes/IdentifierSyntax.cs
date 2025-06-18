using Rich.Lexer;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class IdentifierSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; } = span;
    
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }

    public override string ToString()
    {
        return Span.Text ?? string.Empty;
    }
}