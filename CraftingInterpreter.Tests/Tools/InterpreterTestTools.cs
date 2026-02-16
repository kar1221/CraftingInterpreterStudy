using CraftingInterpreter.Interpret;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Lexing;
using CraftingInterpreter.Parsing;

namespace CraftingInterpreter.Tests.Tools;

public static class InterpreterTestTools
{
    public static InterpreterResult Run(string input, bool isExpression = false)
    {
        var output = new List<string>();
        string? runtimeErrorMessage = null;
        var parseError = false;

        try
        {
            var lexer = new Lexer(input);
            var tokens = lexer.ScanTokens();
            var parser = new Parser(tokens);

            if (isExpression)
            {
                var expr = parser.ParseSingle();
                if (expr == null) return new InterpreterResult("", HadParseError: true);

                var interpreter = new Interpreter(s => output.Add(s));
                interpreter.InterpretSingle(expr);
            }
            else
            {
                var statements = parser.Parse();
                var interpreter = new Interpreter(output.Add);
                interpreter.Interpret(statements);
            }
        }
        catch (RuntimeError error)
        {
            runtimeErrorMessage = error.Message;
        }
        catch (Exception ex)
        {
            runtimeErrorMessage = $"Unexpected System Exception: {ex.Message}";
        }

        return new InterpreterResult(string.Join("\n", output), runtimeErrorMessage, parseError);
    }
}