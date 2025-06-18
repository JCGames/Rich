using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;

public class FunctionSyntax(IdentifierSyntax identifierSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public GenericsListDefinitionSyntax? GenericsListDefinition { get; set; }
    public List<ParameterSyntax> Parameters { get; } = [];
    public TypeSyntax? ReturnType { get; set; }
    public BlockSyntax Body { get; set; } = null!;

    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {Identifier.Span.Text}");
        
        Printer.PrintLine("Returns:");
        Printer.IncreasePadding();
        ReturnType?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Generics:");
        Printer.IncreasePadding();
        GenericsListDefinition?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Parameters:");
        Printer.IncreasePadding();
        foreach (var parameter in Parameters)
        {
            parameter.Print();
        }
        Printer.DecreasePadding();
        
        Body.Print();
    }
}