using Fractals.Parser.SyntaxNodes.Expressions;

namespace Fractals.Parser.SyntaxNodes;

public static class AccessorChainLinkExtensions
{
    public static bool IsValidPathOrImport(this IAccessorChainLink accessorChain)
    {
        if (accessorChain.IsIdentifier) return true;
        if (!accessorChain.IsAccessor) return false;
        
        var current = (AccessorSyntax)accessorChain;
        
        while (true)
        {
            if (current.Left?.IsIdentifier is not true) return false;
            if (current.Right?.IsAccessor is not true) break;
            
            current = (AccessorSyntax)current.Right;
        }

        return current.Right?.IsIdentifier is true;
    }
}