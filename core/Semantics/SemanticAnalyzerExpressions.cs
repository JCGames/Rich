using Rich.Diagnostics;
using Rich.Parser.SyntaxNodes;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Semantics;

public partial class SemanticAnalyzer
{
    private TypeDefinitionSyntax? VisitSyntaxInExpression(Syntax? syntax)
    {
        if (syntax is null) return null;

        return syntax switch
        {
            ExpressionSyntax expressionSyntax => Visit(expressionSyntax),
            BinaryOperatorSyntax binaryOperatorSyntax => Visit(binaryOperatorSyntax),
            UnaryOperatorSyntax unaryOperatorSyntax => Visit(unaryOperatorSyntax),
            AccessorChainSyntax accessorChainSyntax => Visit(accessorChainSyntax)?.LastOrDefault(),
            IntegerSyntax integerSyntax => Visit(integerSyntax),
            DecimalSyntax decimalSyntax => Visit(decimalSyntax),
            StringLiteralSyntax stringLiteralSyntax => Visit(stringLiteralSyntax),
            BooleanSyntax booleanSyntax => Visit(booleanSyntax),
            _ => throw new Exception($"{syntax.GetType().Name} is not supported in {nameof(VisitSyntaxInExpression)}.")
        };
    }
    
    private TypeDefinitionSyntax? Visit(ExpressionSyntax expressionSyntax)
    {
        return VisitSyntaxInExpression(expressionSyntax.Root);
    }

    private TypeDefinitionSyntax? Visit(BinaryOperatorSyntax binaryOperatorSyntax)
    {
        var leftType = VisitSyntaxInExpression(binaryOperatorSyntax.Left);
        var rightType = VisitSyntaxInExpression(binaryOperatorSyntax.Right);

        if (leftType != rightType)
        {
            Report.Error($"Type mismatch in expression: {leftType?.Identifier.Span.Text} <-> {rightType?.Identifier.Span.Text}.",
                binaryOperatorSyntax.OperatorSpan);
        }
        
        return leftType;
    }
    
    private TypeDefinitionSyntax? Visit(UnaryOperatorSyntax unaryOperatorSyntax)
    {
        return VisitSyntaxInExpression(unaryOperatorSyntax.Operand);
    }
    
    private Stack<TypeDefinitionSyntax>? Visit(AccessorChainSyntax accessorChainSyntax)
    {
        var stack = new Stack<TypeDefinitionSyntax>();

        for (var i = 0; i < accessorChainSyntax.Chain.Count; i++)
        {
            switch (accessorChainSyntax.Chain[i])
            {
                case FunctionCallSyntax functionCallSyntax:
                {
                    if (stack.Count > 0) ReopenScope(stack.Peek());
                    
                    var resolvedFunction = i == 0 ? 
                        ResolveFunction(functionCallSyntax) : 
                        ResolveFunctionOnType(functionCallSyntax, stack.Peek());
                    
                    if (stack.Count > 0) CloseReopenedScope();
                    
                    if (resolvedFunction is null)
                    {
                        Report.Error(
                            stack.Count > 0
                                ? $"Missing function {functionCallSyntax.Identifier.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                : $"Missing function {functionCallSyntax.Identifier.Span.Text}.",
                            functionCallSyntax.Identifier.Span);

                        return null;
                    }
                    
                    functionCallSyntax.Binding = resolvedFunction;

                    if (resolvedFunction.ReturnType is not null)
                    {
                        if (resolvedFunction.ReturnType.Binding is not null)
                        {
                            stack.Push(resolvedFunction.ReturnType.Binding);
                        }
                        else
                        {
                            var resolvedType = ResolveType(resolvedFunction.ReturnType);
                            
                            if (resolvedType is null)
                            {
                                Report.Error(
                                    stack.Count > 0
                                        ? $"Cannot find type {resolvedFunction.ReturnType.Span.Text} for function {resolvedFunction.Identifier.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                        : $"Cannot find type {resolvedFunction.Identifier.Span.Text} for function {resolvedFunction.Identifier.Span.Text}.",
                                    resolvedFunction.Identifier.Span);
                                return null;
                            }
      
                            stack.Push(resolvedType);
                            resolvedFunction.ReturnType.Binding = stack.Peek();
                        }
                    }
                    else
                    {
                        stack.Push(LanguageDefinedTypes.NothingType);
                    }
                }
                    break;
                case IndexorSyntax indexorSyntax:
                {
                    if (stack.Count > 0) ReopenScope(stack.Peek());
                    
                    var resolvedVariable = i == 0 ? 
                        ResolveVariable(indexorSyntax.Identifier) : 
                        ResolveVariableOnType(indexorSyntax.Identifier, stack.Peek());
                    
                    if (resolvedVariable is null)
                    {
                        Report.Error(
                            stack.Count > 0
                                ? $"Missing variable {indexorSyntax.Identifier.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                : $"Missing variable {indexorSyntax.Identifier.Span.Text}.",
                            indexorSyntax.Identifier.Span);
                    }
                    else if (resolvedVariable.Type is not null)
                    {
                        var resolvedType = ResolveType(resolvedVariable.Type);

                        if (resolvedType is null)
                        {
                            Report.Error(
                                stack.Count > 0
                                    ? $"Cannot find type {resolvedVariable.Type.Span.Text} for variable {resolvedVariable.Identifier.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                    : $"Cannot find type {resolvedVariable.Type.Span.Text} for variable {resolvedVariable.Identifier.Span.Text}.",
                                resolvedVariable.Identifier.Span);
                            return null;
                        }

                        stack.Push(resolvedType);
                    }
                    
                    indexorSyntax.Binding = resolvedVariable;
                    
                    if (stack.Count > 0) CloseReopenedScope();
                }
                    break;
                case IdentifierSyntax identifierSyntax:
                {
                    if (stack.Count > 0) ReopenScope(stack.Peek());
                    
                    var resolvedVariable = i == 0 ? 
                        ResolveVariable(identifierSyntax) : 
                        ResolveVariableOnType(identifierSyntax, stack.Peek());

                    if (resolvedVariable is null)
                    {
                        Report.Error(
                            stack.Count > 0
                                ? $"Missing variable {identifierSyntax.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                : $"Missing variable {identifierSyntax.Span.Text}.",
                            identifierSyntax.Span);
                    }
                    else if (resolvedVariable.Type is not null)
                    {
                        var resolvedType = ResolveType(resolvedVariable.Type);

                        if (resolvedType is null)
                        {
                            Report.Error(
                                stack.Count > 0
                                    ? $"Cannot find type {resolvedVariable.Type.Span.Text} for variable {resolvedVariable.Identifier.Span.Text} on type {stack.Peek().Identifier.Span.Text}."
                                    : $"Cannot find type {resolvedVariable.Type.Span.Text} for variable {resolvedVariable.Identifier.Span.Text}.",
                                resolvedVariable.Identifier.Span);
                            return null;
                        }

                        stack.Push(resolvedType);
                    }
                    
                    identifierSyntax.Binding = resolvedVariable;
                    
                    if (stack.Count > 0) CloseReopenedScope();
                }
                    break;
                default:
                    throw new Exception("Could not resolve link in accessor chain.");
            }
        }

        return stack;
    }
    
    private TypeDefinitionSyntax? Visit(IntegerSyntax integerSyntax)
    {
        integerSyntax.Binding = LanguageDefinedTypes.IntegerType;
        return integerSyntax.Binding;
    }
    
    private TypeDefinitionSyntax? Visit(DecimalSyntax decimalSyntax)
    {
        decimalSyntax.Binding = LanguageDefinedTypes.DecimalType;
        return decimalSyntax.Binding;
    }
    
    private TypeDefinitionSyntax? Visit(StringLiteralSyntax stringLiteralSyntax)
    {
        stringLiteralSyntax.Binding = LanguageDefinedTypes.StringType;
        return stringLiteralSyntax.Binding;
    }
    
    private TypeDefinitionSyntax? Visit(BooleanSyntax booleanSyntax)
    {
        booleanSyntax.Binding = LanguageDefinedTypes.BooleanType;
        return booleanSyntax.Binding;
    }
}