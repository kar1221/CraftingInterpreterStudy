using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.LoxConsole;
using CraftingInterpreter.TokenModels;
using Environment = CraftingInterpreter.Env.Environment;

namespace CraftingInterpreter.Interpret;

public class Interpreter(Action<string>? writer = null) : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    private Environment _environment = new();

    public void InterpretSingle(Expr expression)
    {
        try
        {
            var value = Evaluate(expression);

            writer?.Invoke(Stringify(value));
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
                if (statement is Stmt.Expression exprStmt)
                {
                    var value = Evaluate(exprStmt.Expr);
                    writer?.Invoke(Stringify(value));
                }
                else
                {
                    Execute(statement);
                }
            }
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    #region Expression Ast

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        _environment.Assign(expr.Name, value);
        return value;
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

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr.Right);
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

    public object VisitVariableExpr(Expr.Variable expr)
    {
        var variable = _environment.Get(expr.Name);

        if (variable == null)
            throw new RuntimeError("Attempt to access uninitialized variable.", expr.Name);

        return variable;
    }

    #endregion


    private object? Evaluate(Expr? expression)
    {
        return expression?.Accept(this);
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

    #region Statement

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        writer?.Invoke(Stringify(value));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        var condition = stmt.Condition;

        if (IsTruthy(Evaluate(condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        try
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                try
                {
                    Execute(stmt.Body);
                }
                catch (ContinueError)
                {
                    if (stmt.Body is Stmt.Block { Statements.Count: > 0 } block)
                    {
                        var last = block.Statements[^1];

                        if (last is Stmt.ForIncrement forIncrement)
                        {
                            Evaluate(forIncrement.IncrementExpr);
                        }
                    }
                }
            }
        }
        catch (BreakError)
        {
        }

        return null;
    }

    public object? VisitForIncrementStmt(Stmt.ForIncrement stmt)
    {
        Evaluate(stmt.IncrementExpr);
        return null;
    }

    public object VisitBreakStmt(Stmt.Break stmt)
    {
        throw new BreakError();
    }

    public object VisitContinueStmt(Stmt.Continue stmt)
    {
        throw new ContinueError();
    }

    #endregion


    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        var previous = _environment;

        try
        {
            _environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }
}