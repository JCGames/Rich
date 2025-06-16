namespace Fractals.Parser.SyntaxNodes;

public class GenericsListDefinitionSyntax : Syntax
{
    public List<IdentifierSyntax> Identifiers { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        foreach (var identifier in Identifiers)
        {
            identifier.Print();
        }
    }
}