using Rich.Diagnostics;
using Rich.Lexer;
using Rich.Parser;
using Rich.Semantics;

#if DEBUG // only run this in debug mode
var lexer = new Lexer(new FileInfo("test.rich"));
#else // run this in release mode
if (args.Length == 0) return;

var buildInTypesLexer = new Lexer(new FileInfo("builtintypes.rich"));
var lexer = new Lexer(new FileInfo(args[0]));
#endif
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
var parser = new Parser();
var ast = parser.Run(tokens);

#if DEBUG
ast.Root?.Print();
#endif

if (Report.Dump())
{
    return;
}

SemanticAnalyzer.Run([ast]);

if (Report.Dump())
{
    return;
}