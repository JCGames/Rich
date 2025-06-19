using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;

public class VariableDeclarationSyntax(IdentifierSyntax identifier, TypeSyntax? type, ExpressionSyntax? expressionSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifier;
    public TypeSyntax? Type { get; set; } = type;
    public ExpressionSyntax? Expression { get; } = expressionSyntax;
    
    public override void Print()
    {
        PrintName();
        
        Identifier.Print();
        
        Printer.PrintLine("Type:");
        Printer.IncreasePadding();
        Type?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Assigned To:");
        Printer.IncreasePadding();
        Expression?.Print();
        Printer.DecreasePadding();
    }
}