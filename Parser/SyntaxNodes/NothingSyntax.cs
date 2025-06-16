using Fractals.Lexer;

namespace Fractals.Parser.SyntaxNodes;
 
public class NothingSyntax : Syntax
{
    public override void Print()
    {
        PrintName();
    }
}