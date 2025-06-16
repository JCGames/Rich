namespace Fractals.Parser.SyntaxNodes.Expressions;

public interface IAccessorChainLink
{
    bool IsIdentifier { get; }
    bool IsAccessor { get; }
    bool IsFunctionCall { get; }
    bool IsIndexor { get; }
    void Print();
}