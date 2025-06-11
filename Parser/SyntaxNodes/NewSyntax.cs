namespace Fractals.Parser.SyntaxNodes;

public class NewSyntax(FunctionCallSyntax functionCallSyntax) : Syntax
{
    public FunctionCallSyntax FunctionCall { get; set; } = functionCallSyntax;
    
    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        FunctionCall.Print();
        
        Printer.DecreasePadding();
    }
}