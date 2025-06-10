using Fractals.Parser;

namespace Fractals.Compiler;

public interface ICompiler
{
    public void Compile(SyntaxTree syntaxTree);
}