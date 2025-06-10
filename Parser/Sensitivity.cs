namespace Fractals.Parser;

[Flags]
public enum Sensitivity : byte
{
    None = 0,
    NewLines = 1 << 0,
    Whitespace = 1 << 1
}