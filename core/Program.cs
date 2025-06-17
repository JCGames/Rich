using Rich.Diagnostics;
using Rich.Lexer;
using Rich.Parser;
using Rich.Semantics;

#if DEBUG
var lexer = new Lexer(new FileInfo("test.rich"));
#else
if (args.Length == 0) return;

var lexer = new Lexer(new FileInfo(args[0]));
#endif
var parser = new Parser();
var tokens = lexer.Run();

#if DEBUG
foreach (var token in tokens)
{
    Console.WriteLine(token);
}
#endif

if (Report.Dump())
{
    return;
}

Console.WriteLine();
var ast = parser.Run(tokens);

#if DEBUG
ast.Root?.Print();
#endif

if (Report.Dump())
{
    return;
}

SemanticAnalyzer.Analyze([ast]);

if (Report.Dump())
{
    return;
}