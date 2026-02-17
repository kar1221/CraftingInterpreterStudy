using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.Interpret;
using CraftingInterpreter.Interpret.Errors;
using CraftingInterpreter.Resolution.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Resolution;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
            Resolve(statement);
    }

    private void Resolve(Stmt? statement)
    {
        statement?.Accept(this);
    }

    public void Resolve(Expr? expression)
    {
        expression?.Accept(this);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        int depth = 0;

        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, depth);
                return;
            }

            depth++;
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType functionType)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = functionType;
        
        BeginScope();
        foreach (var parameter in function.Params)
        {
            Declare(parameter);
            Define(parameter);
        }

        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0)
            return;

        var scope = _scopes.Peek();

        if (!scope.TryAdd(name.Lexeme, false))
        {
            throw new ResolutionError(name, "Already a variable with this name in this scope.");
        }
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0)
            return;

        _scopes.Peek()[name.Lexeme] = true;
    }


    private void EndScope()
    {
        _scopes.Pop();
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        if (_currentClass == ClassType.None)
            throw new RuntimeError("Cannot use 'this' outside of class.", expr.Keyword);
        
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitTernaryExpr(Expr.Ternary expr)
    {
        Resolve(expr.Condition);
        Resolve(expr.ThenBranch);
        Resolve(expr.ElseBranch);
        return null;
    }

    public object? VisitCommaExpr(Expr.Comma expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitLambdaExpr(Expr.Lambda expr)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = FunctionType.Lambda;
        
        BeginScope();

        foreach (var parameter in expr.Params)
        {
            Declare(parameter);
            Define(parameter);
        }

        Resolve(expr.Body);

        EndScope();

        _currentFunction = enclosingFunction;
        
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_scopes.Count > 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool isDefined) && !isDefined)
        {
            throw new ResolutionError(expr.Name, "Cannot read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;
        
        Declare(stmt.Name);
        Define(stmt.Name);
        
        BeginScope();
        _scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            FunctionType declaration = FunctionType.Method;

            if (method.Name.Lexeme == "init")
                declaration = FunctionType.Initializer;
            
            ResolveFunction(method, declaration);
        }
        
        EndScope();

        _currentClass = enclosingClass;
        
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.Function);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
            Resolve(stmt.Initializer);
        Define(stmt.Name);

        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);

        if (stmt.ElseBranch != null)
            Resolve(stmt.ElseBranch);

        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        
        if(stmt.Increment != null)
            Resolve(stmt.Increment);
        
        Resolve(stmt.Body);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (_currentFunction == FunctionType.None)
            throw new ResolutionError(stmt.Keyword, "Cannot return from top-level code");

        if (stmt.Value != null)
        {
            if (_currentFunction == FunctionType.Initializer)
                throw new RuntimeError("Can't return value from an initializer.", stmt.Keyword);
            
            Resolve(stmt.Value);
        }

        return null;
    }

    public object? VisitBreakStmt(Stmt.Break stmt)
    {
        return null;
    }

    public object? VisitContinueStmt(Stmt.Continue stmt)
    {
        return null;
    }
}

internal enum FunctionType
{
    None,
    Function,
    Lambda,
    Method,
    Initializer
}

internal enum ClassType
{
    None,
    Class
}