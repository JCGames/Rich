using Fractals.Lexer;
using Fractals.Parser.SyntaxNodes.Expressions;

namespace Fractals.Parser.SyntaxNodes;

public class IdentifierSyntax(SpanMeta span) : Syntax, IAccessorChainLink
{
    public SpanMeta Span { get; } = span;
    
    public override void Print()
    {
        Printer.PrintLine($"{GetType().Name}: {Span.Text}");
    }

    public bool IsIdentifier => true;
    public bool IsAccessor => false;
    public bool IsFunctionCall => false;
    public bool IsIndexor => false;
}