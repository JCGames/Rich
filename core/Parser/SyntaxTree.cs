using Rich.Parser.SyntaxNodes;

namespace Rich.Parser;

public class SyntaxTree
{
    public string? FilePath { get; set; }
    public BlockSyntax? Root { get; set; }
}