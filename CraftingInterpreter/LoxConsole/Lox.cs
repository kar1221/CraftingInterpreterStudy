using CraftingInterpreter.Interpret;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Resolution;
using CraftingInterpreter.Resolution.Errors;
using Spectre.Console;

namespace CraftingInterpreter.LoxConsole;

using Lexing;
using Parsing;
using TokenModels;

public static class Lox
{
    private static bool _hadError;
    private static bool _hadRuntimeError;

    private static readonly Interpreter Interpreter = new(Console.WriteLine);

    public static void Run(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64);
                break;
            case 1:
                RunFile(args[0]);
                break;
            default:
                RunPrompt();
                break;
        }

    }

    private static void RunFile(string path)
    {
        var file = File.ReadAllText(path);
        RunCommand(file);

        if (_hadError)
            Environment.Exit(65);

        if (_hadRuntimeError)
            Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            var line = AnsiConsole.Ask<string>(">");

            if (line == null)
                break;

            RunCommand(line);
            _hadError = false;
        }
    }

    private static void RunCommand(string source)
    {
        try
        {
            var lexer = new Lexer(source);
            var tokens = lexer.ScanTokens();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            var resolver = new Resolver(Interpreter);
            resolver.Resolve(statements);

            Interpreter.Interpret(statements);
        }
        catch (RuntimeError e)
        {
            RuntimeError(e);
        }
        catch (ResolutionError e)
        {
            Error(e.Token, e.Message);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }


    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, "at end", message);
        }
        else
        {
            Report(token.Line, "at '" + token.Lexeme + "'", message);
        }
    }

    public static void RuntimeError(RuntimeError e)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]✗ Error at line {e.Token?.Line}[/]: {e.Message}");
        _hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]✗ Error at line {line} {where}[/]:  {message}");
        _hadError = true;
    }
}
