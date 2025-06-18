using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public class Scope
{
    public Scope? Parent { get; set; }
    public Dictionary<string, TypeDefinitionSyntax> Types { get; } = [];
    public Dictionary<string, FunctionSyntax> Functions { get; } = [];
    public Dictionary<string, VariableDeclarationSyntax> Variables { get; } = [];
    
    /// <returns>Null if type was added, otherwise returns the type that is already added.</returns>
    public TypeDefinitionSyntax? TryAddType(string typeName, TypeDefinitionSyntax typeDefinitionSyntax)
    {
        if (!Types.TryAdd(typeName, typeDefinitionSyntax))
            return Types[typeName];
        
        return null;
    }
    
    /// <returns>Null if function was added, otherwise returns the function that is already added.</returns>
    public FunctionSyntax? TryAddFunction(string functionName, FunctionSyntax functionSyntax)
    {
        if (!Functions.TryAdd(functionName, functionSyntax))
            return Functions[functionName];
        
        return null;
    }
    
    /// <returns>Null if variable was added, otherwise returns the variable that is already added.</returns>
    public VariableDeclarationSyntax? TryAddVariable(string variableName, VariableDeclarationSyntax variableDeclarationSyntax)
    {
        if (!Variables.TryAdd(variableName, variableDeclarationSyntax))
            return Variables[variableName];
        
        return null;
    }
}