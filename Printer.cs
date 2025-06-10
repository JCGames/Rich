namespace Fractals;

public static class Printer
{
    private static int _padding = 0;
    
    public static void IncreasePadding() => _padding++;

    public static void DecreasePadding()
    {
        if (_padding > 0)
        {
            _padding--;
        }
    }

    public static void PrintLine(string text)
    {
        Console.WriteLine(GetPadding() + text);
    }
    
    public static void Print(string text)
    {
        Console.Write(GetPadding() + text);
    }

    public static void Reset()
    {
        _padding = 0;
    }
    
    private static string GetPadding()
    {
        var padding = string.Empty;
        
        for (var i = 0; i < _padding; i++)
        {
            padding += '\t';
        }

        return padding;
    }
}