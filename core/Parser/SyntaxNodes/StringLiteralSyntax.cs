using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;

public class StringLiteralSyntax(SpanMeta span) : Syntax
{
    public SpanMeta Span { get; init; } = span;
    public TypeDefinitionSyntax? Binding { get; set; }
    
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }
}