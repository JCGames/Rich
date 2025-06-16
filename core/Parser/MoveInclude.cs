namespace Rich.Parser;

[Flags]
public enum MoveInclude : byte
{
    None = 0,
    NewLines = 1 << 0,
    Whitespace = 1 << 1
}