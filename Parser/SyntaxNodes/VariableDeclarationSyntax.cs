using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class VariableDeclarationSyntax(IdentifierSyntax? identifier, TypeSyntax? type, ExpressionSyntax expressionSyntax) : Syntax
{
    public IdentifierSyntax? Identifier { get; init; } = identifier;
    public TypeSyntax? Type { get; set; } = type;
    public ExpressionSyntax Expression { get; init; } = expressionSyntax;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Identifier?.Print();
        Type?.Print();
        Printer.IncreasePadding();
        
        Expression.Print();
        
        Printer.DecreasePadding();
    }
}