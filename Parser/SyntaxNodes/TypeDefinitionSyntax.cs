namespace Fractals.Parser.SyntaxNodes;

public class TypeDefinitionSyntax(IdentifierSyntax identifierSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; init; } = identifierSyntax;
    public List<VariableDeclarationSyntax> Variables { get; set; } = [];
    public List<FunctionSyntax> Functions { get; } = [];
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Identifier.Print();

        Printer.IncreasePadding();
        
        foreach (var v in Variables)
        {
            v.Print();
        }

        foreach (var f in Functions)
        {
            f.Print();
        }
        
        Printer.DecreasePadding();
    }
}