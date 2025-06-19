using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;

public class BooleanSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; } = span;
    public TypeDefinitionSyntax? Binding { get; set; }

    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }
}