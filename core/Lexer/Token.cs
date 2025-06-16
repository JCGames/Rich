namespace Rich.Lexer;

public class Token
{
    public TokenType Type { get; set; }
    public SpanMeta Span { get; }

    public Token(TokenType type, SpanMeta span)
    {
        Type = type;
        Span = span;
    }

    public override string ToString()
    {
        return $"Type: {Type}, {Span}";
    }
}