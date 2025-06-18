using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public class Scope
{
    public Scope? Parent { get; set; }
    public Dictionary<string, TypeDefinitionSyntax> Types { get; } = [];
    public Dictionary<string, FunctionSyntax> Functions { get; } = [];
    public Dictionary<string, VariableDeclarationSyntax> Variables { get; } = [];

    public bool TryAddType(string typeName, TypeDefinitionSyntax typeDefinitionSyntax, out TypeDefinitionSyntax? alreadyExistingTypeDefinition)
    {
        var result = FindTypeInScopes(typeName);
        alreadyExistingTypeDefinition = result.typeDefinitionSyntax;
        
        if (result.success)
        {
            return false;
        }
        
        Types.Add(typeName, typeDefinitionSyntax);

        return true;
    }

    private (bool success, TypeDefinitionSyntax? typeDefinitionSyntax) FindTypeInScopes(string typeName)
    {
        if (Types.TryGetValue(typeName, out var type))
        {
            return (true, type);
        }
        
        if (Parent is not null)
        {
            return Parent.FindTypeInScopes(typeName);
        }

        return (false, null);
    }
    
    public bool TryAddFunction(string functionName, FunctionSyntax functionSyntax, out FunctionSyntax? alreadyExistingFunction)
    {
        var result = FindFunctionInScopes(functionName);
        alreadyExistingFunction = result.functionSyntax;
        
        if (result.success)
        {
            return false;
        }
        
        Functions.Add(functionName, functionSyntax);

        return true;
    }

    private (bool success, FunctionSyntax? functionSyntax) FindFunctionInScopes(string functionName)
    {
        if (Functions.TryGetValue(functionName, out var type))
        {
            return (true, type);
        }
        
        if (Parent is not null)
        {
            return Parent.FindFunctionInScopes(functionName);
        }

        return (false, null);
    }
    
    public bool TryAddVariable(string typeName, VariableDeclarationSyntax variableDeclarationSyntax, out VariableDeclarationSyntax? alreadyExistingVariableDeclaration)
    {
        var result = FindVariableInScopes(typeName);
        alreadyExistingVariableDeclaration = result.variableDeclarationSyntax;
        
        if (result.success)
        {
            return false;
        }
        
        Variables.Add(typeName, variableDeclarationSyntax);
    
        return true;
    }
    
    private (bool success, VariableDeclarationSyntax? variableDeclarationSyntax) FindVariableInScopes(string typeName)
    {
        if (Variables.TryGetValue(typeName, out var variable))
        {
            return (true, variable);
        }
        
        if (Parent is not null)
        {
            return Parent.FindVariableInScopes(typeName);
        }
    
        return (false, null);
    }
}