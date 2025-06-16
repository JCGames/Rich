namespace Rich.Parser.SyntaxNodes;

public class ExpressionSyntax(Syntax? root) : Syntax
{
    public Syntax? Root { get; } = root;
    
    public override void Print()
    {
        PrintName();
        
        Printer.IncreasePadding();
        Root?.Print();
        Printer.DecreasePadding();
    }
}