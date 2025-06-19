using Rich.Diagnostics;
using Rich.Parser;
using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public static class SemanticAnalyzer
{
    public static void Run(List<SyntaxTree> syntaxTrees)
    {
        foreach (var syntaxTree in syntaxTrees)
        {
            // here is where you would see if the syntax tree has a path
            
            if (syntaxTree.Root is null) continue;
        }
    }
}