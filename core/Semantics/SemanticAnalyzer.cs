using Rich.Diagnostics;
using Rich.Parser;
using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public class SemanticAnalyzer
{
    public static Scope GlobalScope { get; } = new();

    public static void Run(List<SyntaxTree> syntaxTrees)
    {
        foreach (var syntaxTree in syntaxTrees)
        {
            // here is where you would see if the syntax tree has a path
            
            if (syntaxTree.Root is null) continue;
            
            CollectTypeDefinitions(syntaxTree.Root, GlobalScope);
        }
    }

    private static void CollectTypeDefinitions(BlockSyntax blockSyntax, Scope scope)
    {
        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is TypeDefinitionSyntax typeDefinitionSyntax)
            {
                var typeName = typeDefinitionSyntax.Identifier.Span.Text +
                               typeDefinitionSyntax.GenericsListDefinition?.ToNumberOfGenericsString();
                
                if (!scope.TryAddType(typeName, typeDefinitionSyntax, out var alreadyExistingTypeDefinition))
                {
                    Report.Error($"The type {typeName} on line {typeDefinitionSyntax.Identifier.Span.Line} is already defined here.", alreadyExistingTypeDefinition?.Identifier.Span);
                }
            }
        }
    }
}