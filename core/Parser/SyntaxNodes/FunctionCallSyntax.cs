using Rich.Lexer;
using Rich.Parser.SyntaxNodes.Expressions;

namespace Rich.Parser.SyntaxNodes;

public class FunctionCallSyntax(IdentifierSyntax identifierSyntax) : Syntax
{
    public IdentifierSyntax Identifier { get; } = identifierSyntax;
    public TypeListSyntax? TypeList { get; set; }
    public List<ExpressionSyntax> Arguments { get; } = [];
    public FunctionSyntax? Binding { get; set; }
    
    public override void Print()
    {
        PrintName();
        
        Printer.PrintLine($"Name: {identifierSyntax.Span.Text}");
        
        TypeList?.Print();
        
        Printer.PrintLine("Arguments: [");
        Printer.IncreasePadding();
        foreach (var argument in Arguments)
        {
            argument.Print();
        }
        Printer.DecreasePadding();
        Printer.PrintLine("]");
    }
}