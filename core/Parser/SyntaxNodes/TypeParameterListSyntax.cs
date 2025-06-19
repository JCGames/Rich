namespace Rich.Parser.SyntaxNodes;

public class TypeParameterListSyntax : Syntax
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

    public override string ToString()
    {
        return $"<{string.Join(',', Identifiers)}>";
    }
}