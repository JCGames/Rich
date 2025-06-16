using Rich.Lexer;

namespace Rich.Parser.SyntaxNodes;
 
public class NothingSyntax : Syntax
{
    public override void Print()
    {
        PrintName();
    }
}