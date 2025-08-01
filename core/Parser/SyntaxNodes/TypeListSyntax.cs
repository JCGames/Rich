namespace Rich.Parser.SyntaxNodes;

public class TypeListSyntax : Syntax
{
    public List<TypeSyntax> Types { get; } = [];
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        foreach (var generic in Types)
        {
            generic.Print();
        }
        Printer.DecreasePadding();
    }
}