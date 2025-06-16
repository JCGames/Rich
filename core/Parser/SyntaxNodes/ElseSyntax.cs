namespace Rich.Parser.SyntaxNodes;

public class ElseSyntax : LikeIfSyntax
{
    public BlockSyntax Body { get; init; } = null!;
    
    public override void Print()
    {
        PrintName();
        
        Body.Print();
    }
}