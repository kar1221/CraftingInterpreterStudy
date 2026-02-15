namespace CraftingInterpreter.AbstractSyntaxTree;

public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
{
    public string Print(Expr expression)
    {
        var result = expression.Accept(this);

        return result ?? "";
    }

    public string Print(Stmt stmt)
    {
        var result = stmt.Accept(this);

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

    public string VisitVariableExpr(Expr.Variable expr)
    {
        return Parenthesize("Var", expr);
    }

    private string Parenthesize(string name, params Expr[] arguments) =>
        $"({name} {string.Join(" ", arguments.Select(e => e.Accept(this)))})";

    private string Parenthesize(string name, params Stmt[] arguments) =>
        $"({name} {string.Join(" ", arguments.Select(e => e.Accept(this)))})";

    public string VisitBlockStmt(Stmt.Block stmt)
    {
        return Parenthesize("block", stmt.Statements.ToArray());
    }

    public string VisitExpressionStmt(Stmt.Expression stmt)
    {
        return Parenthesize("expr", stmt.Expr);
    }

    public string VisitPrintStmt(Stmt.Print stmt)
    {
        return Parenthesize("print ", stmt.Expr);
    }

    public string VisitVarStmt(Stmt.Var stmt)
    {
        return Parenthesize($"var {stmt.Name.Lexeme}", stmt.Initializer!);
    }
}