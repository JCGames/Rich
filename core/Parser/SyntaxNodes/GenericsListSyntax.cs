namespace Rich.Parser.SyntaxNodes;

public class GenericsListSyntax : Syntax
{
    public List<TypeSyntax> Generics { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        foreach (var generic in Generics)
        {
            generic.Print();
        }
        Printer.DecreasePadding();
    }
}