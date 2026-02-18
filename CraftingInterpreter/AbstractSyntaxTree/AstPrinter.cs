using System.Text;

namespace CraftingInterpreter.AbstractSyntaxTree;

public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
{
    public string Print(Expr expression)
    {
        var result = expression.Accept(this);

        return result ?? "";
    }

    public string VisitAssignExpr(Expr.Assign expr)
    {
        return Parenthesize("Assignment", expr);
    }

    public string VisitBinaryExpr(Expr.Binary expression)
    {
        return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
    }

    public string VisitCallExpr(Expr.Call expr)
    {
        return Parenthesize("Call", expr);
    }

    public string VisitGetExpr(Expr.Get expr)
    {
        return Parenthesize("get", expr);
    }

    public string VisitGroupingExpr(Expr.Grouping expression)
    {
        return Parenthesize("group", expression.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expression)
    {
        if (expression.Value == null)
            return "nil";

        return expression.Value.ToString() ?? "";
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        return Parenthesize("logical", expr);
    }

    public string? VisitThisExpr(Expr.This expr)
    {
        return Parenthesize("this");
    }

    public string? VisitSetExpr(Expr.Set expr)
    {
        return Parenthesize("set", expr);
    }

    public string VisitUnaryExpr(Expr.Unary expression)
    {
        return Parenthesize(expression.Operator.Lexeme, expression.Right);
    }

    public string VisitTernaryExpr(Expr.Ternary expression)
    {
        return Parenthesize("?", expression.Condition, expression.ThenBranch, expression.ElseBranch);
    }

    public string VisitCommaExpr(Expr.Comma expression)
    {
        return Parenthesize(",", expression.Left, expression.Right);
    }

    public string VisitLambdaExpr(Expr.Lambda expr)
    {
        return Parenthesize("lambda", expr.Body, expr.Params);
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        return Parenthesize("Var", expr);
    }

    private string Parenthesize(string name, params object?[] parts)
    {
        var builder = new StringBuilder();
        builder.Append('(').Append(name);

        foreach (var part in parts)
        {
            if (part == null)
                continue;

            builder.Append(' ');
            builder.Append(part switch
            {
                Expr expr => expr.Accept(this),
                Stmt stmt => stmt.Accept(this),
                Stmt[] stmt => string.Join(", ", stmt.Select(s => s.Accept(this))),
                null => "nil",
                _ => part.ToString() // Handles Tokens, Strings, and Numbers
            });
        }

        builder.Append(')');
        return builder.ToString();
    }


    public string VisitBlockStmt(Stmt.Block stmt)
    {
        return Parenthesize("block", [.. stmt.Statements]);
    }

    public string VisitClassStmt(Stmt.Class stmt)
    {
        return Parenthesize("class", stmt);
    }

    public string VisitExpressionStmt(Stmt.Expression stmt)
    {
        return Parenthesize("expr", stmt.Expr);
    }

    public string VisitFunctionStmt(Stmt.Function stmt)
    {
        return Parenthesize("Func", stmt);
    }

    public string VisitPrintStmt(Stmt.Print stmt)
    {
        return Parenthesize("print ", stmt.Expr);
    }

    public string VisitVarStmt(Stmt.Var stmt)
    {
        return Parenthesize($"var {stmt.Name.Lexeme}", stmt.Initializer);
    }

    public string VisitIfStmt(Stmt.If stmt)
    {
        return Parenthesize("if", stmt.Condition, stmt.ThenBranch, stmt.ElseBranch);
    }

    public string VisitWhileStmt(Stmt.While stmt)
    {
        return Parenthesize("while", stmt.Condition, stmt.Body);
    }

    public string VisitReturnStmt(Stmt.Return stmt)
    {
        return Parenthesize("return", stmt.Keyword, stmt.Value);
    }

    public string VisitBreakStmt(Stmt.Break stmt)
    {
        return Parenthesize("break");
    }

    public string VisitContinueStmt(Stmt.Continue stmt)
    {
        return Parenthesize("continue");
    }
}
