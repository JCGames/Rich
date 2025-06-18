namespace Rich.Parser.SyntaxNodes.Extensions;

public static class AccessorSyntaxExtensions
{
    public static bool IsOnlyIdentifiers(this AccessorChainSyntax accessorChainSyntax)
    {
        return accessorChainSyntax.Chain.All(syntax => syntax is IdentifierSyntax);
    }
    
    public static bool IsRightMostAFunctionCall(this AccessorChainSyntax? accessorSyntax)
    {
        return accessorSyntax?.Chain.LastOrDefault() is FunctionCallSyntax;
    }
}