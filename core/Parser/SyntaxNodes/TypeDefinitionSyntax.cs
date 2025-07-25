namespace Rich.Parser.SyntaxNodes;

public class TypeDefinitionSyntax(IdentifierSyntax identifierSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public List<VariableDeclarationSyntax> Variables { get; } = [];
    public List<FunctionSyntax> Functions { get; } = [];
    public TypeParameterListSyntax? TypeParameterList { get; set; }
    
    public override void Print()
    {
        PrintName();

        Identifier.Print();
        
        Printer.PrintLine("Generics:");
        Printer.IncreasePadding();
        TypeParameterList?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Fields:");
        Printer.IncreasePadding();
        foreach (var v in Variables)
        {
            v.Print();
        }
        Printer.DecreasePadding();

        Printer.PrintLine("Functions:");
        Printer.IncreasePadding();
        foreach (var f in Functions)
        {
            f.Print();
        }
        Printer.DecreasePadding();
    }
}