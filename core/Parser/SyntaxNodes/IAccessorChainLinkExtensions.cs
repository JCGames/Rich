using Rich.Lexer;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

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

    public static string ToDisplayString(this IAccessorChainLink accessorChain)
    {
        var displayString = string.Empty;
        var current = accessorChain;

        while (current is AccessorSyntax accessor)
        {
            switch (accessor.Left)
            {
                case IdentifierSyntax identifier:
                    displayString += identifier.Span.Text;
                    break;
                case FunctionCallSyntax functionCall:
                    displayString += functionCall.Identifier.Span.Text + "()";
                    break;
                case IndexorSyntax indexor:
                    displayString += indexor.Identifier.Span.Text + "[]";
                    break;
            }

            current = accessor.Right;
            
            displayString += '.';
        }
        
        switch (current)
        {
            case IdentifierSyntax identifier2:
                displayString += identifier2.Span.Text;
                break;
            case FunctionCallSyntax functionCall2:
                displayString += functionCall2.Identifier.Span.Text + "()";
                break;
            case IndexorSyntax indexor2:
                displayString += indexor2.Identifier.Span.Text + "[]";
                break;
        }

        return displayString;
    }

    public static SpanMeta? GetSpan(this IAccessorChainLink accessorChain)
    {
        SpanMeta? spanMeta = null;
        var current = accessorChain;

        while (current is AccessorSyntax accessor)
        {
            switch (accessor.Left)
            {
                case IdentifierSyntax identifier:
                    if (spanMeta is null)
                    {
                        spanMeta = identifier.Span;
                    }
                    else
                    {
                        spanMeta.Combine(identifier.Span);
                    }
                    break;
                case FunctionCallSyntax functionCall:
                    if (spanMeta is null)
                    {
                        spanMeta = functionCall.Identifier.Span;
                    }
                    else
                    {
                        spanMeta.Combine(functionCall.Identifier.Span);
                    }
                    break;
                case IndexorSyntax indexor:
                    if (spanMeta is null)
                    {
                        spanMeta = indexor.Identifier.Span;
                    }
                    else
                    {
                        spanMeta.Combine(indexor.Identifier.Span);
                    }
                    break;
            }

            current = accessor.Right;
        }
        
        switch (current)
        {
            case IdentifierSyntax identifier:
                if (spanMeta is null)
                {
                    spanMeta = identifier.Span;
                }
                else
                {
                    spanMeta.Combine(identifier.Span);
                }
                break;
            case FunctionCallSyntax functionCall:
                if (spanMeta is null)
                {
                    spanMeta = functionCall.Identifier.Span;
                }
                else
                {
                    spanMeta.Combine(functionCall.Identifier.Span);
                }
                break;
            case IndexorSyntax indexor:
                if (spanMeta is null)
                {
                    spanMeta = indexor.Identifier.Span;
                }
                else
                {
                    spanMeta.Combine(indexor.Identifier.Span);
                }
                break;
        }

        return spanMeta;
    }
}