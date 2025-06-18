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
            CollectFunctions(syntaxTree.Root, GlobalScope);
            CollectVariables(syntaxTree.Root, GlobalScope);
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
                    var otherLocation = alreadyExistingTypeDefinition?.Identifier.Span.FilePath + ":" + alreadyExistingTypeDefinition?.Identifier.Span.Line;
                    Report.Error($"The type {typeName} is already defined at {otherLocation}.", typeDefinitionSyntax.Identifier.Span);
                }
            }
        }
    }

    private static void CollectFunctions(BlockSyntax blockSyntax, Scope scope)
    {
        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is FunctionSyntax functionSyntax)
            {
                var functionName = functionSyntax.Identifier.Span.Text +
                                   functionSyntax.GenericsListDefinition?.ToNumberOfGenericsString();

                if (!scope.TryAddFunction(functionName, functionSyntax, out var alreadyExistingFunction))
                {
                    var otherLocation = alreadyExistingFunction?.Identifier.Span.FilePath + ":" + alreadyExistingFunction?.Identifier.Span.Line;
                    Report.Error($"The function {functionName} is already defined at {otherLocation}.", functionSyntax.Identifier.Span);
                }
            }
        }
    }

    private static void CollectVariables(BlockSyntax blockSyntax, Scope scope)
    {
        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is VariableDeclarationSyntax variableDeclarationSyntax)
            {
                var variableName = variableDeclarationSyntax.Identifier.Span.Text ?? throw new Exception("AHHHHH");
                
                if (!scope.TryAddVariable(variableName, variableDeclarationSyntax, out var alreadyExistingVariable))
                {
                    var otherLocation = alreadyExistingVariable?.Identifier.Span.FilePath + ":" + alreadyExistingVariable?.Identifier.Span.Line;
                    Report.Error($"The variable {variableName} is already defined at {otherLocation}.", variableDeclarationSyntax.Identifier.Span);
                }
            }
        }
    }
}