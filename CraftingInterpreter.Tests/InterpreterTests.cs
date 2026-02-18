using CraftingInterpreter.Tests.Tools;
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
    [TestCase("1, 2, 3, nil", "")]
    public static void Interpreter_Comma_ShouldReturnRightMostOperand(string input, string expected)
    {
        AssertExpression(input, expected);
    }

    [TestCase("1 + 2", "3")]
    [TestCase("1 - 2", "-1")]
    [TestCase("1 - 1 / 2", "0.5")]
    [TestCase("1 < 2", "True")]
    [TestCase("\"2\" + 2", "22")]
    [TestCase("1 < 2 < 3", "Operands must be numbers.")]
    [TestCase("1 / 0", "Cannot divide by zero.")]
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

    [TestCase("var a = 1; print a += 1;", "2")]
    [TestCase("var a = 1; print a -= 1;", "0")]
    [TestCase("var a = 1; print a *= 2;", "2")]
    [TestCase("var a = 1; print a /= 2;", "0.5")]
    public void Interpreter_IncrementDecrement_ShouldIncreaseOrDecreaseValue(string input, string expected)
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
        "Cannot read local variable in its own initializer.")
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
        "Cannot read local variable in its own initializer.")
    ]
    [TestCase("""
              var a = 1;
              {
                var a = 2;
                var a = 3;
              }
              """,
            "Already a variable with this name in this scope.") // Throws
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


    [TestCase("""
              print true and "no";
              """, "no")]
    [TestCase("""
              print false and "no";
              """, "False")]
    [TestCase("print 1 and 2 and 3;", "3")]
    [TestCase("print nil and 2 and 3;", "")]
    [TestCase("print nil or 2 or 3;", "2")]
    [TestCase("print nil or nil or 3;", "3")]
    public void Interpreter_Logical_ShouldSuccessfullyEvaluate(string input, string expected)
    {
        AssertStatement(input, expected);
    }


    [CancelAfter(2000)]
    [TestCase("""
              var i = 0;
              while (i < 5) { print i; i = i + 1; }
              """, "0\n1\n2\n3\n4")]
    [TestCase("for(var i = 0; i < 5; i = i + 1) { print i; }", "0\n1\n2\n3\n4")]
    [TestCase("for(var i = 0; i < 5; i += 1) { print i; }", "0\n1\n2\n3\n4")]
    [TestCase("for(var i = 5; i > 0; i -= 1) { print i; }", "5\n4\n3\n2\n1")]
    public void Interpreter_Loop_ShouldRunSuccessfully(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    [CancelAfter(2000)]
    [TestCase("""
              var i = 0;
              while (i < 10) {
                if (i == 3) break;
                print i;
                i = i + 1;
              }
              """, "0\n1\n2")]
    [TestCase("""
              var i = 0;
              while (i < 5) {
                i = i + 1;
                if (i == 3) continue;
                print i;
              }
              """, "1\n2\n4\n5")]
    public void Interpreter_WhileControlFlow_ShouldBreakAndContinue(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    [CancelAfter(2000)]
    [TestCase("""
              for (var i = 0; i < 5; i = i + 1) {
                if (i == 2) break;
                print i;
              }
              """, "0\n1")]
    [TestCase("""
              for (var i = 0; i < 5; i = i + 1) {
                if (i == 2) continue;
                print i;
              }
              """, "0\n1\n3\n4")]
    public void Interpreter_ForControlFlow_ShouldBreakAndContinue(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    [CancelAfter(2000)]
    [TestCase("""
              for (var i = 0; i < 2; i = i + 1) {
                for (var j = 0; j < 10; j = j + 1) {
                    if (j == 2) break;
                    print i + "-" + j;
                }
              }
              """, "0-0\n0-1\n1-0\n1-1")]
    public void Interpreter_NestedLoops_ShouldOnlyBreakInnermost(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    [TestCase("""
              var i = 0;
              while (i < 3) {
                i = i + 1;
                {
                    {
                        if (i == 2) continue;
                    }
                }
                print i;
              }
              """, "1\n3")]
    public void Interpreter_DeeplyNestedControlFlow_ShouldJumpToLoop(string input, string expected)
    {
        AssertStatement(input, expected);
    }


    [TestCase("""
              fun add(a, b) {
                print a + b;
              }

              add(1, 2);
              """, "3")]
    [TestCase("""
              fun add(a, b) {
                return a + b;
              }

              print add(1, 2);
              """, "3")]
    [TestCase("""
              fun add_factory(a) {
                return fun(b) {
                    return a + b;
                };
              }

              print add_factory(2)(2);
              """, "4")]
    [TestCase("""
              fun add_closure(a) {
                 for(var i = 0; i < 5; i += 1)
                    print a(i);
              }

              add_closure((a) => a + 1);
              """, "1\n2\n3\n4\n5")]
    [TestCase("""
              fun add_closure(a) {
                 for(var i = 0; i < 5; i += 1)
                    if (i % 2 == 0)
                        print i;
              }

              add_closure(5);
              """, "0\n2\n4")]
    public void Interpreter_Function(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    [TestCase("""
              fun do_something(fn) {
                var i = 2;
                fn(i);
              }

              do_something(fun (i) { print i; });
              """, "2")]
    [TestCase("""
              fun do_something(fn) {
                var i = 2;
                fn(i);
              }

              do_something((i) => { print i; });
              """, "2")]
    public void Interpreter_Lambdas(string input, string expected)
    {
        AssertStatement(input, expected);
    }


    [TestCase("""
              class Cake {
                  taste() {
                      var adjective = "delicious";
                      print "The " + this.flavor + " cake is " + adjective + "!";
                  }
              }
              var cake = Cake();
              cake.flavor = "German chocolate";
              cake.taste(); 
              """, "German chocolate\nThe German chocolate cake is delicious!")]
    [TestCase("""
              class Cake {
                  init() {
                      print "Init invoked";
                  }
              }
              var cake = Cake();
              """, "Init invoked")]
    [TestCase("""
              class Math {
                   class square(n) {
                        return n * n;
                   }
              }
              print Math.square(2);
              """, "4")]
    [TestCase("""
              class Square {
                   init(width) {
                        this.width = width;
                   }

                   area {
                        return this.width * this.width;
                   }
              }
              var square = Square(5);
              print square.area;
              """, "25")]
    [TestCase("""
              class Square {
                   init(width) {
                        this.width = width;
                   }

                   width(v) {
                        this.width = v;
                   }

                   area {
                        return this.width * this.width;
                   }
              }
              var square = Square(5);
              print square.area;
              square.width = 10;
              print square.area;
              """, "25\n10\n100")]
    public void Interpreter_Class(string input, string expected)
    {
        AssertStatement(input, expected);
    }

    private static void AssertExpression(string input, string expected)
    {
        var result = Run(input, isExpression: true);
        HandleAssertion(result, expected, input);
    }

    private static void AssertStatement(string input, string expected)
    {
        var result = Run(input, isExpression: false);
        HandleAssertion(result, expected, input);
    }

    private static void HandleAssertion(InterpreterResult result, string expected, string input)
    {
        if (result.RuntimeError != null)
        {
            Assert.That(result.RuntimeError, Is.EqualTo(expected), $"Expected RuntimeError mismatch for: {input}");
        }
        else
        {
            Assert.That(result.Output, Is.EqualTo(expected), $"Output mismatch for: {input}");
        }
    }
}
