using CraftingInterpreter.Lexing;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Tests;

[TestFixture]
public class LexerTests
{
    [Test]
    public static void Lexer_Comment_ShouldSkipSingleLineComments()
    {
        var lexer = new Lexer("// A Comment");
        var tokens = lexer.ScanTokens();

        Assert.That(tokens, Has.Count.EqualTo(1));
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }


    [Test]
    public static void Lexer_Comment_ShouldSkipBlockComments()
    {
        var lexer = new Lexer("""
                              /* this is a
                                multiline comment
                              */
                              """);
        var tokens = lexer.ScanTokens();

        Assert.That(tokens, Has.Count.EqualTo(1));
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }


    [Test]
    public static void Lexer_Comment_ShouldWorkForNestedBlockComments()
    {
        var lexer = new Lexer("""
                              /* this is a
                                    /* nested comments */
                              */
                              """);
        var tokens = lexer.ScanTokens();

        Assert.That(tokens, Has.Count.EqualTo(1));
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public static void Lexer_Token_ShouldIdentifyConsecutiveTokenCorrectly()
    {
        var lexer = new Lexer("<===!=");

        var expectedToken = new List<TokenType>()
        {
            TokenType.LessEqual, TokenType.EqualEqual,
            TokenType.BangEqual,
            TokenType.Eof
        };

        var tokens = lexer.ScanTokens();

        Assert.That(tokens, Has.Count.EqualTo(4));

        for (var i = 0; i < tokens.Count; i++)
        {
            Assert.That(tokens[i].Type, Is.EqualTo(expectedToken[i]));
        }
    }
}