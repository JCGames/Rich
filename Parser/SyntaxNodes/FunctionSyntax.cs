using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;

public class FunctionSyntax(SpanMeta nameSpan) : Syntax
{
    public SpanMeta NameSpan { get; } = nameSpan;
    public List<ParameterSyntax> Parameters { get; } = [];
    public TypeSyntax? ReturnType { get; set; }
    public BlockSyntax Body { get; set; } = null!;
    public GenericsListDefinitionSyntax? GenericsListDefinition { get; set; }

    public override void Print()
    {
        Printer.PrintLine(GetType().Name);
        Printer.IncreasePadding();
        
        Printer.PrintLine($"Name: {NameSpan.Text}");
        ReturnType?.Print();
        GenericsListDefinition?.Print();
        Printer.PrintLine("Parameters: [");
        foreach (var parameter in Parameters)
        {
            parameter.Print();
        }
        Printer.PrintLine("]");
        Body.Print();
        
        Printer.DecreasePadding();
    }
}