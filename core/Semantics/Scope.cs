using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public class Scope
{
    public Dictionary<string, TypeDefinitionSyntax> Types { get; } = [];
    public Dictionary<string, FunctionSyntax> Functions { get; } = [];
    public Dictionary<string, VariableDeclarationSyntax> Variables { get; } = [];
}