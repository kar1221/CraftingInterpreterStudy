using static CraftingInterpreter.Tests.Tools.InterpreterTestTools;

namespace CraftingInterpreter.Tests;

[TestFixture]
public class InterpreterTests
{
    [TestCase("1 ? 2 : 3", "2")]
    [TestCase("nil ? 2 : 3", "3")]
    public static void Interpreter_Ternary_ShouldCorrectlyEvaluate(string input, string expected)
    {
        AssertExpression(input, expected);
    }

    [TestCase("1, 2, 3", "3")]
    [TestCase("1, 2, 3, nil", "nil")]
    public static void Interpreter_Comma_ShouldReturnRightMostOperand(string input, string expected)
    {
        AssertExpression(input, expected);
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
        AssertExpression(input, expected);
    }

    [TestCase("var a = 1; print a;", "1")]
    [TestCase("var a = \"hello\"; print a + \" world\";", "hello world")]
    public void Interpreter_VarStmt_ShouldDefineVariables(string input, string expected)
    {
        AssertStatement(input, expected);
    }


    [TestCase("""
              var a = 1;
              {
                var a = a + 2;
                print a;
              }
              print a;
              """,
        "3\n1")
    ]
    [TestCase("""
              var a = 1;
              var a = 2;
              print a;
              {
                var a = a + 2;
                print a;
              }
              print a;
              """,
        "2\n4\n2")
    ]
    [TestCase("""
              var a = 1;
              {
                var a = 2;
                var a = 3;
              }
              """,
        "") // Throws
    ]
    [TestCase("""
              var a = 1;
              var a = 2;
              {
                print a;
              }
              """,
            "2")
    ]
    public void Interpreter_Scoping_ShouldFollowScopingRule(string input, string expected)
    {
        AssertStatement(input, expected);
    }
    
    [TestCase("""
              var a = true ? 1 : 2; 
              print a;
              """, "1")] 
    [TestCase("""
              var a = false ? 1 : true ? 2 : 3; 
              print a;
              """, "2")] 
    [TestCase("""
              var a = false ? 1 : false ? 2 : 3; 
              print a;
              """, "3")] 
    public void Interpreter_Ternary_ShouldSuccessfullyEvaluate(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    private static void AssertExpression(string input, string expected)
    {
        var result = EvaluateExpression(input);
        Assert.That(result, Is.Not.Null, $"Failed for input: {input}");
        Assert.That(result, Is.EqualTo(expected), $"Failed for input: {input}");
    }


    private static void AssertStatement(string input, string expected)
    {
        var result = EvaluateStatement(input);
        Assert.That(result, Is.Not.Null, $"Failed for input: {input}");
        Assert.That(result, Is.EqualTo(expected), $"Failed for input: {input}");
    }
}