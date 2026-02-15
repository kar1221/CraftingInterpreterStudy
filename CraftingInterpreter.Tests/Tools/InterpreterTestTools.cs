using CraftingInterpreter.Interpret;
using CraftingInterpreter.Lexing;
using CraftingInterpreter.Parsing;

namespace CraftingInterpreter.Tests.Tools;

public static class InterpreterTestTools
{
    public static string? EvaluateExpression(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer.ScanTokens());
        var expression = parser.ParseSingle();

        if (expression == null)
            return null;

        var output = "";
        var interpreter = new Interpreter(s => output = s);
        interpreter.InterpretSingle(expression);

        return output;
    }
    
    public static string EvaluateStatement(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer.ScanTokens());
        var expression = parser.Parse();

        var output = new List<string>();
        var interpreter = new Interpreter(output.Add);
        interpreter.Interpret(expression);

        return string.Join("\n", output);
    }
    
}