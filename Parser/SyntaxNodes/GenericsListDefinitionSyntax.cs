namespace Fractals.Parser.SyntaxNodes;

public class GenericsListDefinitionSyntax : Syntax
{
    public List<IdentifierSyntax> Identifiers { get; set; } = [];
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();

        foreach (var identifier in Identifiers)
        {
            identifier.Print();
        }
        
        Printer.DecreasePadding();
    }
}