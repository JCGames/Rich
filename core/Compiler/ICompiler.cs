using Rich.Parser;

namespace Rich.Compiler;

public interface ICompiler
{
    public void Compile(SyntaxTree syntaxTree);
}