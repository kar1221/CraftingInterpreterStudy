using CraftingInterpreter.Interpret;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Lexing;
using CraftingInterpreter.Parsing;
using CraftingInterpreter.Resolution;

namespace CraftingInterpreter.Tests.Tools;

public static class InterpreterTestTools
{
    public static InterpreterResult Run(string input, bool isExpression = false)
    {
        var output = new List<string>();
        string? runtimeErrorMessage = null;
        var parseError = false;

        var interpreter = new Interpreter(output.Add);

        try
        {
            var lexer = new Lexer(input);
            var tokens = lexer.ScanTokens();
            var parser = new Parser(tokens);
            var resolver = new Resolver(interpreter);

            if (isExpression)
            {
                var expr = parser.ParseSingle();
                resolver.Resolve(expr);
                if (expr == null) return new InterpreterResult("", HadParseError: true);
                interpreter.InterpretSingle(expr);
            }
            else
            {
                var statements = parser.Parse();
                resolver.Resolve(statements);
                interpreter.Interpret(statements);
            }
        }
        catch (RuntimeError error)
        {
            runtimeErrorMessage = error.Message;
        }
        catch (Exception ex)
        {
            runtimeErrorMessage = ex.Message;
        }

        return new InterpreterResult(string.Join("\n", output), runtimeErrorMessage, parseError);
    }
}