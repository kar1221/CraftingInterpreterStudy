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

}