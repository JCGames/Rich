namespace Fractals.Parser.SyntaxNodes.Expressions;

public class AccessorSyntax : Syntax, IAccessorChainLink
{
    public IAccessorChainLink? Left { get; set; }
    public IAccessorChainLink? Right { get; set; }
    
    public bool IsIdentifier => false;
    public bool IsAccessor => true;
    public bool IsFunctionCall => false;
    public bool IsIndexor => false;
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine("Left:");
        Printer.IncreasePadding();
        Left?.Print();
        Printer.DecreasePadding();
        
        Printer.PrintLine("Right:");
        Printer.IncreasePadding();
        Right?.Print();
        Printer.DecreasePadding();
    }
}