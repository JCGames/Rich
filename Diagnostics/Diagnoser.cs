using Fractals.Lexer;

namespace Fractals.Diagnostics;

public static class Diagnoser
{
    private static readonly List<(string message, SpanMeta span)> _errors = [];
    
    public static void AddError(string message, SpanMeta span)
    {
        _errors.Add((message, span));
        
// #if DEBUG
//         throw new Exception(message);
// #endif
    }

    public static bool Dump()
    {
        if (_errors.Any())
        {
            foreach (var (message, span) in _errors)
            {
                var normal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"{span.FilePath}:({span.Line},{span.Column})");
                Console.WriteLine($"\t{message}");
                Console.WriteLine();
                
                Console.ForegroundColor = normal;
            }

            return true;
        }

        return false;
    }
}