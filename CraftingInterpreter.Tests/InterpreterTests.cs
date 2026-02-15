using static CraftingInterpreter.Tests.Tools.TokenTestTools;

namespace CraftingInterpreter.Tests;

[TestFixture]
public class InterpreterTests
{
    [TestCase("1 ? 2 : 3", "2")]
    [TestCase("nil ? 2 : 3", "3")]
    public static void Interpreter_Ternary_ShouldCorrectlyEvaluate(string input, string expected)
    {
        AssertEvaluation(input, expected);
    }

    [TestCase("1, 2, 3", "3")]
    [TestCase("1, 2, 3, nil", "nil")]
    public static void Interpreter_Comma_ShouldReturnRightMostOperand(string input, string expected)
    {
        AssertEvaluation(input, expected);
    }

    [TestCase("1 + 2", "3")]
    [TestCase("1 - 2", "-1")]
    [TestCase("1 - 1 / 2", "0.5")]
    [TestCase("1 < 2", "True")]
    [TestCase("\"2\" + 2", "22")]
    [TestCase("1 < 2 < 3", "")]
    [TestCase("1 / 0", "")]
    public static void Interpreter_Binary_ShouldReturnExpectedResult(string input, string expected)
    {
        AssertEvaluation(input, expected);
    }

    private static void AssertEvaluation(string input, string expected)
    {
        var result = EvaluateString(input);
        Assert.That(result, Is.Not.Null, $"Failed for input: {input}");
        Assert.That(result, Is.EqualTo(expected), $"Failed for input: {input}");
    }
}