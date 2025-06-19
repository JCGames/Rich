using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes.Expressions;

public enum UnaryOperatorKind
{
    Negation,
    Not
}

public class UnaryOperatorSyntax(SpanMeta operatorSpan, UnaryOperatorKind kind) : Syntax
{
    public SpanMeta OperatorSpan { get; init; } = operatorSpan;
    public UnaryOperatorKind Kind { get; } = kind;
    public Syntax? Operand { get; init; }
    
    public override void Print()
    {
        PrintName();
        Printer.PrintLine($"Kind: {Kind}");
        
        Printer.IncreasePadding();
        Operand?.Print();
        Printer.DecreasePadding();
    }
}