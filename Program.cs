using Fractals.Compiler.Cpp;
using Fractals.Diagnostics;
using Fractals.Lexer;
using Fractals.Parser;

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

if (Diagnoser.Dump())
{
    return;
}

Console.WriteLine();
var ast = parser.Run(tokens);

#if DEBUG
ast.Root?.Print();
#endif

if (Diagnoser.Dump())
{
    return;
}

// var compiler = new CppCompiler();
//
// compiler.Compile(ast);
//
// Diagnoser.Dump();