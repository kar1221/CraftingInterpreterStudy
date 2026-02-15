using CraftingInterpreter.Interpret;
using CraftingInterpreter.Lexing;
using CraftingInterpreter.Parsing;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Tests.Tools;

public static class TokenTestTools
{
    public static Token SimpleToken(TokenType type, string lexeme, object? literal = null)
    {
        return new Token(type, lexeme, literal, 1);
    }

    public static List<Token> ToList(params Token[] tokens)
    {
        var list = tokens.ToList();
        list.Add(new Token(TokenType.Eof, "", null, 1));
        return list;
    }

    public static string? EvaluateString(string input)
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
}