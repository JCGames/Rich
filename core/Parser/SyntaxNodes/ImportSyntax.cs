using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class ImportSyntax(SpanMeta importSpan) : Syntax
{
    public SpanMeta ImportSpan { get; init; } = importSpan;
    
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {ImportSpan.Text}");
    }
}