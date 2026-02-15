using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.LoxConsole;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Interpret;

public class Interpreter(Action<string> writer) : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    public void InterpretSingle(Expr expression)
    {
        try
        {
            var value = Evaluate(expression);
            writer(Stringify(value));
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    public object? VisitBinaryExpr(Expr.Binary expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        switch (expression.Operator.Type)
        {
            case TokenType.Greater:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! > (double)right!;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! >= (double)right!;
            case TokenType.Less:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! < (double)right!;
            case TokenType.LessEqual:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! <= (double)right!;
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
            case TokenType.Minus:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! - (double)right!;
            case TokenType.Slash:
                CheckNumberOperands(expression.Operator, left, right);

                if (right is double and 0)
                    throw new RuntimeError("Cannot divide by zero", expression.Operator);

                return (double)left! / (double)right!;
            case TokenType.Star:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! * (double)right!;
            case TokenType.Plus:
                return left switch
                {
                    double a when right is double b => a + b,
                    string a when right is string b => a + b,
                    string a when right is double b => a + b,
                    double a when right is string b => a + b,
                    _ => throw new RuntimeError("Invalid Operands", expression.Operator)
                };
            default:
                return null;
        }
    }

    public object? VisitGroupingExpr(Expr.Grouping expression)
    {
        return Evaluate(expression.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expression)
    {
        return expression.Value;
    }

    public object? VisitUnaryExpr(Expr.Unary expression)
    {
        var right = Evaluate(expression.Right);

        switch (expression.Operator.Type)
        {
            case TokenType.Bang:
                return IsTruthy(right);
            case TokenType.Minus:
                CheckNumberOperand(expression.Operator, right);
                return -(double)right!;
        }

        return null;
    }

    public object? VisitTernaryExpr(Expr.Ternary expression)
    {
        var result = Evaluate(expression.Condition);

        return Evaluate(IsTruthy(result) ? expression.ThenBranch : expression.ElseBranch);
    }

    public object? VisitCommaExpr(Expr.Comma expression)
    {
        Evaluate(expression.Left);

        return Evaluate(expression.Right);
    }

    private object? Evaluate(Expr expression)
    {
        return expression.Accept(this);
    }

    private static bool IsTruthy(object? o)
    {
        return o switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsEqual(object? a, object? b)
    {
        return a switch
        {
            null when b == null => true,
            null => false,
            _ => a.Equals(b)
        };
    }

    private static void CheckNumberOperand(Token @operator, object? operand)
    {
        if (operand is double)
            return;

        throw new RuntimeError("Operand must be a number.", @operator);
    }

    private static void CheckNumberOperands(Token @operator, object? left, object? right)
    {
        if (left is double && right is double)
            return;

        throw new RuntimeError("Operands must be numbers", @operator);
    }

    private static string Stringify(object? o)
    {
        switch (o)
        {
            case null:
                return "nil";
            case double:
            {
                var text = o.ToString()!;
                if (text.EndsWith(".0"))
                {
                    text = text[..^2];
                }

                return text;
            }
            default:
                return o.ToString()!;
        }
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }
}