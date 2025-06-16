using System.Diagnostics;
using Fractals.Diagnostics;
using Fractals.Parser;
using Fractals.Parser.SyntaxNodes;
using Fractals.Parser.SyntaxNodes.Expressions;

namespace Fractals.Compiler.Cpp;

public class CppCompiler : ICompiler
{
    private StreamWriter? _streamWriter;
    
    public void Compile(SyntaxTree syntaxTree)
    {
        _streamWriter = new StreamWriter("out.cpp");
        
        CompileImports(syntaxTree.Root);
        DefineStandardLibrary();
        
        CompileGlobalVariableDeclarations(syntaxTree.Root);
        CompileFunctions(syntaxTree.Root);
        
        _streamWriter.WriteLine("int main(int argc, char** argv){");
        CompileSyntax(syntaxTree.Root, isTopLevel: true);
        _streamWriter.WriteLine("return 0;");
        _streamWriter.WriteLine("}");
        _streamWriter.Close();

        Console.WriteLine("Compiling...");
        
        var fileName = Path.GetFileNameWithoutExtension(syntaxTree.FilePath) ?? "main";
        
        var compileProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = $"/C g++ out.cpp -o {fileName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        compileProcess.Start();

        var output = compileProcess.StandardOutput.ReadToEnd();
        var error = compileProcess.StandardError.ReadToEnd();

        compileProcess.WaitForExit();
        
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine(error);
        }
        
        // Console.WriteLine("Cleaning up...");
        
        // var deleteProcess = new Process
        // {
        //     StartInfo = new ProcessStartInfo
        //     {
        //         FileName = "cmd.exe",
        //         WorkingDirectory = Environment.CurrentDirectory,
        //         Arguments = "/C del out.cpp",
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true,
        //         UseShellExecute = false,
        //         CreateNoWindow = true
        //     }
        // };
        //
        // deleteProcess.Start();
        //
        // var output3 = compileProcess.StandardOutput.ReadToEnd();
        // var error3 = compileProcess.StandardError.ReadToEnd();
        //
        // deleteProcess.WaitForExit();
        //
        // Console.WriteLine(output3);
        //
        // if (!string.IsNullOrEmpty(error3))
        // {
        //     Console.WriteLine(error3);
        // }
    }

    private void DefineStandardLibrary()
    {
        _streamWriter?.WriteLine("#include <iostream>");
        _streamWriter?.WriteLine("#include <string>");
        _streamWriter?.WriteLine("std::string input() { std::string result; std::getline(std::cin, result); return result; }");
        _streamWriter?.WriteLine("void print(std::string text) { std::cout << text; }");
        _streamWriter?.WriteLine("void println(std::string text) { std::cout << text << std::endl; }");
    }
    
    private void CompileImports(BlockSyntax? blockSyntax)
    {
        if (blockSyntax is null) return;

        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is ImportSyntax importSyntax)
            {
                //_streamWriter?.WriteLine($"#include \"{importSyntax..Span.Text}\"");
            }
        }
    }
    
    private void CompileGlobalVariableDeclarations(BlockSyntax? blockSyntax)
    {
        if (blockSyntax is null) return;

        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is VariableDeclarationSyntax variableDeclarationSyntax)
            {
                CompileVariableDeclaration(variableDeclarationSyntax);
            }
        }
    }

    private void CompileFunctions(BlockSyntax? blockSyntax)
    {
        if (blockSyntax is null) return;
        
        foreach (var syntax in blockSyntax.Children)
        {
            if (syntax is FunctionSyntax functionSyntax)
            {
                var returnTypeName = GetCppEquivalentTypeName(functionSyntax.ReturnType?.Span.Text);
                _streamWriter?.Write($"{returnTypeName} {functionSyntax.NameSpan.Text}(");

                for (var i = 0; i < functionSyntax.Parameters.Count; i++)
                {
                    var typeName = GetCppEquivalentTypeName(functionSyntax.Parameters[i].Type.Span.Text);
                    _streamWriter?.Write($"{typeName} {functionSyntax.Parameters[i].NameSpan.Text}");
                    
                    if (i != functionSyntax.Parameters.Count - 1)
                    {
                        _streamWriter?.Write(",");
                    }
                }
                
                _streamWriter?.WriteLine("){");
                
                CompileSyntax(functionSyntax.Body);
                
                _streamWriter?.WriteLine("}");
            }
        }
    }
    
    private void CompileSyntax(Syntax? syntax, bool isTopLevel = false)
    {
        if (!isTopLevel && syntax is VariableDeclarationSyntax variableDeclarationSyntax)
        {
            CompileVariableDeclaration(variableDeclarationSyntax);
        }
        else if (syntax is WhileSyntax whileSyntax)
        {
            _streamWriter?.Write("while ");
            CompileExpression(whileSyntax.Condition);
            _streamWriter?.WriteLine();
            _streamWriter?.WriteLine("{");
            CompileSyntax(whileSyntax.Body);
            _streamWriter?.WriteLine("}");
        }
        else if (syntax is IfSyntax ifSyntax)
        {
            CompileIf(ifSyntax);
        }
        else if (syntax is FunctionCallSyntax functionCallSyntax)
        {
            CompileFunctionCall(functionCallSyntax);
            _streamWriter?.WriteLine(";");
        }
        else if (syntax is ReturnSyntax returnSyntax)
        {
            _streamWriter?.Write("return ");
            CompileExpression(returnSyntax.Expression, ignoreExpressionParentheses: true);
            _streamWriter?.WriteLine(";");
        }
        else if (syntax is BreakSyntax)
        {
            _streamWriter?.WriteLine("break;");
        }
        else if (syntax is ContinueSyntax)
        {
            _streamWriter?.WriteLine("continue;");
        }
        else if (syntax is AssignmentSyntax assignmentSyntax)
        {
            // _streamWriter?.Write($"{assignmentSyntax.Identifier.Span.Text}=");
            // CompileExpression(assignmentSyntax.Expression, ignoreExpressionParentheses: true);
            // _streamWriter?.WriteLine(";");
        }
        else if (syntax is BlockSyntax blockSyntax)
        {
            foreach (var child in blockSyntax.Children)
            {
                CompileSyntax(child, isTopLevel);
            }
        }
    }
    
    private void CompileVariableDeclaration(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        var typeName = GetCppEquivalentTypeName(variableDeclarationSyntax.Type?.Span.Text);
        _streamWriter?.Write(variableDeclarationSyntax.Type is not null
            ? $"{typeName} "
            : "auto ");

        _streamWriter?.Write($"{variableDeclarationSyntax.Identifier.Span.Text}=");
        CompileExpression(variableDeclarationSyntax.Expression, ignoreExpressionParentheses: true);
        _streamWriter?.WriteLine(";");
    }

    private void CompileFunctionCall(FunctionCallSyntax functionCallSyntax)
    {
        if (functionCallSyntax.NameSpan.Text is "cast")
        {
            if (functionCallSyntax.Arguments.Count != 2)
            {
                Diagnoser.AddError("Wrong number of arguments", functionCallSyntax.NameSpan);
                return;
            }

            if (functionCallSyntax.Arguments[1].Root is not IdentifierSyntax identifierSyntax)
            {
                Diagnoser.AddError("No type to cast to specified.", functionCallSyntax.NameSpan);
                return;
            }

            _streamWriter?.Write($"({GetCppEquivalentTypeName(identifierSyntax.Span.Text)})(");
            CompileExpression(functionCallSyntax.Arguments[0], ignoreExpressionParentheses: true);
            _streamWriter?.Write(")");
            return;
        }
        
        if (functionCallSyntax.NameSpan.Text is "toString")
        {
            if (functionCallSyntax.Arguments.Count != 1)
            {
                Diagnoser.AddError("Wrong number of arguments", functionCallSyntax.NameSpan);
                return;
            }

            _streamWriter?.Write($"std::to_string(");
            CompileExpression(functionCallSyntax.Arguments[0], ignoreExpressionParentheses: true);
            _streamWriter?.Write(")");
            return;
        }
        
        _streamWriter?.Write($"{functionCallSyntax.NameSpan.Text}(");
        for (var i = 0; i < functionCallSyntax.Arguments.Count; i++)
        {
            CompileExpression(functionCallSyntax.Arguments[i], ignoreExpressionParentheses: true);

            if (i != functionCallSyntax.Arguments.Count - 1)
            {
                _streamWriter?.Write(",");
            }
        }
        _streamWriter?.Write(")");
    }

    private void CompileIf(LikeIfSyntax? likeIfSyntax)
    {
        if (likeIfSyntax is IfSyntax ifSyntax)
        {
            _streamWriter?.Write("if ");
            CompileExpression(ifSyntax.Condition);
            _streamWriter?.WriteLine("{");
            CompileSyntax(ifSyntax.Body);
            _streamWriter?.WriteLine("}");

            if (ifSyntax.Branch is not null)
            {
                _streamWriter?.Write("else ");
                CompileIf(ifSyntax.Branch);
            }
        }
        else if (likeIfSyntax is ElseSyntax elseSyntax)
        {
            _streamWriter?.WriteLine("{");
            CompileSyntax(elseSyntax.Body);
            _streamWriter?.WriteLine("}");
        }
    }

    private void CompileExpression(Syntax? syntax, bool ignoreExpressionParentheses = false)
    {
        if (syntax is null) return;
        
        if (syntax is ExpressionSyntax expressionSyntax)
        {
            if (!ignoreExpressionParentheses) _streamWriter?.Write("(");
            CompileExpression(expressionSyntax.Root);
            if (!ignoreExpressionParentheses) _streamWriter?.Write(")");
        }
        else if (syntax is AdditionSyntax additionSyntax)
        {
            CompileExpression(additionSyntax.Left);
            _streamWriter?.Write("+");
            CompileExpression(additionSyntax.Right);
        }
        else if (syntax is SubtractionSyntax subtractionSyntax)
        {
            CompileExpression(subtractionSyntax.Left);
            _streamWriter?.Write("-");
            CompileExpression(subtractionSyntax.Right);
        }
        else if (syntax is MultiplicationSyntax multiplicationSyntax)
        {
            CompileExpression(multiplicationSyntax.Left);
            _streamWriter?.Write("*");
            CompileExpression(multiplicationSyntax.Right);
        }
        else if (syntax is DivisionSyntax divisionSyntax)
        {
            CompileExpression(divisionSyntax.Left);
            _streamWriter?.Write("/");
            CompileExpression(divisionSyntax.Right);
        }
        else if (syntax is ModulusSyntax modulusSyntax)
        {
            CompileExpression(modulusSyntax.Left);
            _streamWriter?.Write("%");
            CompileExpression(modulusSyntax.Right);
        }
        else if (syntax is LogicalAndSyntax logicalAndSyntax)
        {
            CompileExpression(logicalAndSyntax.Left);
            _streamWriter?.Write("&&");
            CompileExpression(logicalAndSyntax.Right);
        }
        else if (syntax is LogicalOrSyntax logicalOrSyntax)
        {
            CompileExpression(logicalOrSyntax.Left);
            _streamWriter?.Write("||");
            CompileExpression(logicalOrSyntax.Right);
        }
        else if (syntax is EqualsSyntax equalsSyntax)
        {
            CompileExpression(equalsSyntax.Left);
            _streamWriter?.Write("==");
            CompileExpression(equalsSyntax.Right);
        }
        else if (syntax is NotEqualsSyntax notEqualsSyntax)
        {
            CompileExpression(notEqualsSyntax.Left);
            _streamWriter?.Write("!=");
            CompileExpression(notEqualsSyntax.Right);
        }
        else if (syntax is GreaterThanSyntax greaterThanSyntax)
        {
            CompileExpression(greaterThanSyntax.Left);
            _streamWriter?.Write(">");
            CompileExpression(greaterThanSyntax.Right);
        }
        else if (syntax is LessThanSyntax lessThanSyntax)
        {
            CompileExpression(lessThanSyntax.Left);
            _streamWriter?.Write("<");
            CompileExpression(lessThanSyntax.Right);
        }
        else if (syntax is GreaterThanOrEqualSyntax greaterThanOrEqualSyntax)
        {
            CompileExpression(greaterThanOrEqualSyntax.Left);
            _streamWriter?.Write(">=");
            CompileExpression(greaterThanOrEqualSyntax.Right);
        }
        else if (syntax is LessThanOrEqualSyntax lessThanOrEqualSyntax)
        {
            CompileExpression(lessThanOrEqualSyntax.Left);
            _streamWriter?.Write("<=");
            CompileExpression(lessThanOrEqualSyntax.Right);
        }
        else if (syntax is BooleanSyntax booleanSyntax)
        {
            _streamWriter?.Write(booleanSyntax.Span.Text);
        }
        else if (syntax is DecimalSyntax decimalSyntax)
        {
            _streamWriter?.Write(decimalSyntax.Span.Text);
        }
        else if (syntax is IdentifierSyntax identifierSyntax)
        {
            _streamWriter?.Write(identifierSyntax.Span.Text);
        }
        else if (syntax is StringLiteralSyntax stringLiteralSyntax)
        {
            _streamWriter?.Write($"\"{stringLiteralSyntax.Span.Text}\"");
        }
        else if (syntax is IntegerSyntax integerSyntax)
        {
            _streamWriter?.Write(integerSyntax.Span.Text);
        }
        else if (syntax is FunctionCallSyntax functionCallSyntax)
        {
            CompileFunctionCall(functionCallSyntax);
        }
    }

    private string GetCppEquivalentTypeName(string? typeName)
    {
        if (typeName is "string")
        {
            return "std::string";
        }
        
        return typeName ?? "void";
    }
}