// See https://aka.ms/new-console-template for more information

using CraftingInterpreter.Lexing;

namespace CraftingInterpreter;

internal static class Lox
{
    private static bool _hadError;

    public static int Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: jlox [script]");
                return 64;
            case 1:
                return RunFile(args[0]);
            default:
                RunPrompt();
                break;
        }

        return 0;
    }

    private static int RunFile(string path)
    {
        var file = File.ReadAllText(path);
        Run(file);

        return _hadError ? 65 : 0;
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null)
                break;

            Run(line);
            _hadError = false;
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.ScanTokens();

        foreach (var t in tokens)
        {
            Console.WriteLine(t);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        _hadError = true;
    }
}