// See https://aka.ms/new-console-template for more information

using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Lexing;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter;

internal static class Lox
{
    private static bool _hadError;

    public static void Main(string[] args)
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
        Run(file);

        if(_hadError)
            Environment.Exit(65);
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