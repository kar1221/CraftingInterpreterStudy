using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Parsing;
using CraftingInterpreter.TokenModels;
using static CraftingInterpreter.Tests.Tools.TokenTestTools;

namespace CraftingInterpreter.Tests;

[TestFixture]
public class ParserTests
{
    private static readonly AstPrinter Printer = new();

    [Test]
    public static void Parse_BinaryAddition_ReturnsCorrectTree()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Number, "1", 1.0),
            SimpleToken(TokenType.Plus, "+"),
            SimpleToken(TokenType.Number, "2", 2.0)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(Printer.Print(result), Is.EqualTo("(+ 1 2)"));
        }
    }

    [Test]
    public static void Parse_Precedence_MultiplicationBeforeAddition()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Number, "1", 1.0),
            SimpleToken(TokenType.Plus, "+"),
            SimpleToken(TokenType.Number, "2", 2.0),
            SimpleToken(TokenType.Star, "*"),
            SimpleToken(TokenType.Number, "3", 3)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(Printer.Print(result), Is.EqualTo("(+ 1 (* 2 3))"));
        }
    }


    [Test]
    public static void Parse_Comma_ShouldEvaluateAndDiscardLeftThenReturnRight()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Number, "1", 1.0),
            SimpleToken(TokenType.Comma, ","),
            SimpleToken(TokenType.Number, "2", 2.0)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(Printer.Print(result), Is.EqualTo("(, 1 2)"));
        }
    }


    [Test]
    public static void Parse_Comma_ShouldWorkForMultipleCommas()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Number, "1", 1.0),
            SimpleToken(TokenType.Comma, ","),
            SimpleToken(TokenType.Number, "2", 2.0),
            SimpleToken(TokenType.Comma, ","),
            SimpleToken(TokenType.Number, "3", 3.0)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(Printer.Print(result), Is.EqualTo("(, (, 1 2) 3)"));
        }
    }

    [Test]
    public static void Parse_Ternary_ShouldHaveCorrectTree()
    {
        var tokens = ToList(
            SimpleToken(TokenType.True, "true", true),
            SimpleToken(TokenType.Question, "?"),
            SimpleToken(TokenType.Number, "1", 1.0),
            SimpleToken(TokenType.Colon, ":"),
            SimpleToken(TokenType.Number, "3", 3.0)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(Printer.Print(result), Is.EqualTo("(? True 1 3)"));
        }
    }


    [Test]
    public static void Parse_MissingLeftOperand_ShouldThrow()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Plus, "+"),
            SimpleToken(TokenType.Number, "3", 3.0)
        );

        var parser = new Parser(tokens);
        var result = parser.ParseSingle();

        Assert.That(result, Is.Null);
    }


    [Test]
    public static void Parse_VarDeclaration_ShouldSuccessfullyParse()
    {
        var tokens = ToList(
            SimpleToken(TokenType.Var, "var"),
            SimpleToken(TokenType.Identifier, "a"),
            SimpleToken(TokenType.Equal, "="),
            SimpleToken(TokenType.Number, "2", 2.0),
            SimpleToken(TokenType.SemiColon, ";")
        );

        var parser = new Parser(tokens);
        var result = parser.Parse();

        var stmt = result[0];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Empty);
            Assert.That(stmt, Is.InstanceOf<Stmt.Var>());
            var text = new AstPrinter().VisitVarStmt((Stmt.Var)stmt);
            Assert.That(text, Is.EqualTo("(var a 2)"));
        }
    }
}