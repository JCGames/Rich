namespace Fractals.Parser.SyntaxNodes;

public class GenericsListSyntax : Syntax
{
    public List<TypeSyntax> Generics { get; init; } = [];
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();

        foreach (var generic in Generics)
        {
            generic.Print();
        }
        
        Printer.DecreasePadding();
    }
}