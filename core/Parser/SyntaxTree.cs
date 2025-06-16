using Fractals.Parser.SyntaxNodes;

namespace Fractals.Parser;

public class SyntaxTree
{
    public string? FilePath { get; set; }
    public BlockSyntax? Root { get; set; }
}