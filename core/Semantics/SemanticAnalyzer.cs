using Rich.Diagnostics;
using Rich.Parser;
using Rich.Parser.SyntaxNodes;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Semantics;

public partial class SemanticAnalyzer
{
    private readonly Dictionary<Syntax, Scope> _scopesTable = [];
    private readonly Stack<Scope> _scopes = [];
    
    private Scope CurrentScope => _scopes.Peek();
    
    public static void Run(List<SyntaxTree> syntaxTrees)
    {
        var analyzer = new SemanticAnalyzer();
        
        foreach (var syntaxTree in syntaxTrees)
        {
            // here is where you would see if the syntax tree has a path
            
            if (syntaxTree.Root is null) continue;
            
            analyzer.Collect(syntaxTree.Root);
        }
        
        foreach (var syntaxTree in syntaxTrees)
        {
            // here is where you would see if the syntax tree has a path
            
            if (syntaxTree.Root is null) continue;
            
            analyzer.Visit(syntaxTree.Root);
        }
    }

    private void OpenScope()
    {
        _scopes.Push(new Scope());
    }
    
    private void ReopenScope(Syntax scopesSyntaxRef)
    {
        if (_scopesTable.TryGetValue(scopesSyntaxRef, out var scope))
        {
            _scopes.Push(scope);
            return;
        }

        throw new Exception("Scope was never opened.");
    }

    private void CloseScope(Syntax scopesSyntax)
    {
        var scope = _scopes.Pop();

        if (!_scopesTable.TryAdd(scopesSyntax, scope))
        {
            throw new Exception("A scope for this syntax has already been created.");
        }
    }

    private void CloseReopenedScope()
    {
        _scopes.Pop();
    }

    private TypeDefinitionSyntax? ResolveType(TypeSyntax typeSyntax)
    {
        var shortName = typeSyntax.Span.Text;

        switch (shortName)
        {
            case "str": return LanguageDefinedTypes.StringType;
            case "bool": return LanguageDefinedTypes.BooleanType;
            case "byte": return LanguageDefinedTypes.ByteType;
            case "int": return LanguageDefinedTypes.IntegerType;
            case "decimal": return LanguageDefinedTypes.DecimalType;
        }
        
        var name = typeSyntax.Span.Text
                   + typeSyntax.TypeList?.ToNumberOfGenericsString();
        
        foreach (var scope in _scopes)
        {
            if (scope.Types.TryGetValue(name, out var result))
            {
                return result;
            }
        }

        return null;
    }
    
    private FunctionSyntax? ResolveFunction(FunctionCallSyntax functionCallSyntax)
    {
        var name = functionCallSyntax.Identifier.Span.Text
                   + functionCallSyntax.TypeList?.ToNumberOfGenericsString();
        
        foreach (var scope in _scopes)
        {
            if (scope.Functions.TryGetValue(name, out var result))
            {
                return result;
            }
        }

        return null;
    }
    
    private VariableDeclarationSyntax? ResolveVariable(IdentifierSyntax identifierSyntax)
    {
        ArgumentNullException.ThrowIfNull(identifierSyntax.Span.Text);
        var name = identifierSyntax.Span.Text;
        
        foreach (var scope in _scopes)
        {
            if (scope.Variables.TryGetValue(name, out var result))
            {
                return result;
            }
        }

        return null;
    }
    
    private VariableDeclarationSyntax? ResolveVariableOnType(IdentifierSyntax identifierSyntax, TypeDefinitionSyntax? typeDefinitionSyntax)
    {
        if (typeDefinitionSyntax is null) return null;
        
        ArgumentNullException.ThrowIfNull(identifierSyntax.Span.Text);
        var name = identifierSyntax.Span.Text;

        if (_scopesTable.TryGetValue(typeDefinitionSyntax, out var scope))
        {
            if (scope.Variables.TryGetValue(name, out var result))
            {
                return result;
            }
        }

        return null;
    }
    
    private FunctionSyntax? ResolveFunctionOnType(FunctionCallSyntax functionCallSyntax, TypeDefinitionSyntax? typeDefinitionSyntax)
    {
        if (typeDefinitionSyntax is null) return null;
        
        var name = functionCallSyntax.Identifier.Span.Text
                   + functionCallSyntax.TypeList?.ToNumberOfGenericsString();

        if (_scopesTable.TryGetValue(typeDefinitionSyntax, out var scope))
        {
            if (scope.Functions.TryGetValue(name, out var result))
            {
                return result;
            }
        }

        return null;
    }

    private void AddTypeToScope(TypeDefinitionSyntax typeDefinitionSyntax)
    {
        var typeName = typeDefinitionSyntax.Identifier.Span.Text
                       + typeDefinitionSyntax.TypeParameterList?.ToNumberOfGenericsString();

        if (!CurrentScope.Types.TryAdd(typeName, typeDefinitionSyntax))
        {
            Report.Error("Type has already been declared.", typeDefinitionSyntax.Identifier.Span);
        }
    }
    
    private void AddFunctionToScope(FunctionSyntax functionSyntax)
    {
        var functionName = functionSyntax.Identifier.Span.Text
                           + functionSyntax.TypeParameterList?.ToNumberOfGenericsString();

        if (!CurrentScope.Functions.TryAdd(functionName, functionSyntax))
        {
            Report.Error("Function has already been declared.", functionSyntax.Identifier.Span);
        }
    }

    private void AddVariableToScope(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        ArgumentNullException.ThrowIfNull(variableDeclarationSyntax.Identifier.Span.Text);
        var variableName = variableDeclarationSyntax.Identifier.Span.Text;
        
        if (!CurrentScope.Variables.TryAdd(variableName, variableDeclarationSyntax))
        {
            Report.Error("Variable has already been declared.", variableDeclarationSyntax.Identifier.Span);
        }
    }
    
    private void Collect(BlockSyntax blockSyntax)
    {
        OpenScope();

        foreach (var syntax in blockSyntax.Children)
        {
            switch (syntax)
            {
                case TypeDefinitionSyntax typeDefinitionSyntax:
                    AddTypeToScope(typeDefinitionSyntax);
                    Collect(typeDefinitionSyntax);
                    break;
                case FunctionSyntax functionSyntax:
                    AddFunctionToScope(functionSyntax);
                    Collect(functionSyntax);
                    break;
                case VariableDeclarationSyntax variableDeclarationSyntax:
                    AddVariableToScope(variableDeclarationSyntax);
                    break;
            }
        }
        
        CloseScope(blockSyntax);
    }

    private void Collect(FunctionSyntax functionSyntax)
    {
        OpenScope();
        
        foreach (var syntax in functionSyntax.Block.Children)
        {
            switch (syntax)
            {
                case TypeDefinitionSyntax typeDefinitionSyntax:
                    Report.Error("Type should not be defined here.", typeDefinitionSyntax.Identifier.Span);
                    break;
                case FunctionSyntax functionSyntax2:
                    Report.Error("Functions should not be defined here.", functionSyntax2.Identifier.Span);
                    break;
                case VariableDeclarationSyntax variableDeclarationSyntax:
                    AddVariableToScope(variableDeclarationSyntax);
                    break;
            }
        }
        
        CloseScope(functionSyntax);
    }

    private void Collect(TypeDefinitionSyntax typeDefinitionSyntax)
    {
        OpenScope();
        
        foreach (var syntax in typeDefinitionSyntax.Variables)
        {
            AddVariableToScope(syntax);
        }

        foreach (var syntax in typeDefinitionSyntax.Functions)
        {
            AddFunctionToScope(syntax);
        }
        
        CloseScope(typeDefinitionSyntax);
    }

    private void Visit(BlockSyntax blockSyntax)
    {
        ReopenScope(blockSyntax);

        foreach (var syntax in blockSyntax.Children)
        {
            switch (syntax)
            {
                case VariableDeclarationSyntax variableDeclarationSyntax:
                    Visit(variableDeclarationSyntax);
                    break;
            }
        }
        
        CloseReopenedScope();
    }

    private void Visit(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        // not an auto variable
        if (variableDeclarationSyntax.Type is not null)
        {
            var resolvedType = ResolveType(variableDeclarationSyntax.Type);
            variableDeclarationSyntax.Type.Binding = resolvedType;

            if (variableDeclarationSyntax.Expression is not null)
            {
                var expressionResultType = VisitSyntaxInExpression(variableDeclarationSyntax.Expression);

                if (resolvedType != expressionResultType)
                {
                    var variableTypeName = resolvedType?.Identifier.Span.Text;
                    var expressionResultTypeName = expressionResultType?.Identifier.Span.Text;
                    Report.Error($"Variable type {variableTypeName} does not match {expressionResultTypeName}.", 
                        variableDeclarationSyntax.Identifier.Span);
                }    
            }
        }
        // is an audio variable
        else
        {
            variableDeclarationSyntax.Type = new TypeSyntax(variableDeclarationSyntax.Identifier.Span)
            {
                Binding = VisitSyntaxInExpression(variableDeclarationSyntax.Expression)
            };
        }
    }
}