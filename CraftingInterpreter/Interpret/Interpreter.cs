using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret.BuiltIn;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Interpret.Interfaces;
using CraftingInterpreter.TokenModels;
using Environment = CraftingInterpreter.Env.Environment;

namespace CraftingInterpreter.Interpret;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    private Environment _environment;
    private readonly Environment _globals;
    private readonly Action<string>? _writer;
    private readonly Dictionary<Expr, int> _locals = new();

    public Interpreter(Action<string>? writer = null)
    {
        _globals = new Environment();
        _environment = _globals;
        _writer = writer;

        _globals.Define("clock", new Clock());
        _globals.Define("time", new Time());
        _globals.Define("date", new Date());
        _globals.Define("exit", new Exit());
    }

    public void InterpretSingle(Expr expression)
    {
        var value = Evaluate(expression);

        Write(value);
    }

    public void Interpret(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            if (statement is Stmt.Expression exprStmt)
            {
                var value = Evaluate(exprStmt.Expr);

                Write(value);
            }
            else
            {
                Execute(statement);
            }
        }
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }

    #region Expression Ast

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);

        if (_locals.TryGetValue(expr, out var distance))
        {
            _environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            _globals.Assign(expr.Name, value);
        }
        
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
            case TokenType.Percent:
                return (double)left! % (double)right!;
            case TokenType.Minus:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left! - (double)right!;
            case TokenType.Slash:
                CheckNumberOperands(expression.Operator, left, right);

                if (right is double and 0)
                    throw new RuntimeError("Cannot divide by zero.", expression.Operator);

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
                    _ => throw new RuntimeError("Invalid Operands.", expression.Operator)
                };
            default:
                return null;
        }
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        if (callee == null)
            return null;

        var arguments = expr.Arguments.Select(argument => Evaluate(argument)!).ToList();

        if (callee is not ICallable function)
            throw new RuntimeError("Can only call function or classes.", expr.Paren);

        if (arguments.Count != function.Arity())
            throw new RuntimeError($"Expected {function.Arity()} arguments but got {arguments.Count} instead.",
                expr.Paren);


        return function.Call(this, arguments);
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        var @object = Evaluate(expr.Object);

        if (@object is LoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError("Only instances have properties.", expr.Name);
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

    public object? VisitThisExpr(Expr.This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        var ob = Evaluate(expr.Object);

        if (ob is not LoxInstance instance)
            throw new RuntimeError("Only instance have fields.", expr.Name);

        var value = Evaluate(expr.Value);
        
        instance.Set(expr.Name, value);

        return value;
    }

    public object? VisitUnaryExpr(Expr.Unary expression)
    {
        var right = Evaluate(expression.Right);

        switch (expression.Operator.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(right);
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

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            return _environment.GetAt(distance, name.Lexeme);
        }
    
        return _globals.Get(name);
    }

    public object VisitLambdaExpr(Expr.Lambda expr)
    {
        return new LoxCallable(expr.Params, expr.Body, _environment);
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

        throw new RuntimeError("Operands must be numbers.", @operator);
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

    private void Write(object? value)
    {
        if (value == null)
            return;

        _writer?.Invoke(Stringify(value));
    }

    #region Statement

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        _environment.Define(stmt.Name.Lexeme, null);

        var methods = new Dictionary<string, LoxCallable>();
        
        foreach (var method in stmt.Methods)
        {
            var function = new LoxCallable(method, _environment);
            methods[method.Name.Lexeme] = function;
        }
        
        var @class = new LoxClass(stmt.Name.Lexeme, methods);
        _environment.Assign(stmt.Name, @class);
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxCallable(stmt, _environment);
        _environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Write(value);
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

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;

        if (stmt.Value != null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
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
                    
                    if(stmt.Increment != null)
                        Evaluate(stmt.Increment);
                }
                catch (Continue)
                {
                    if(stmt.Increment != null)
                        Evaluate(stmt.Increment);
                }
            }
        }
        catch (Break)
        {
        }

        return null;
    }

    public object VisitBreakStmt(Stmt.Break stmt)
    {
        throw new Break();
    }

    public object VisitContinueStmt(Stmt.Continue stmt)
    {
        throw new Continue();
    }

    #endregion


    private void Execute(Stmt? stmt)
    {
        if (stmt == null)
            return;
        
        stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
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