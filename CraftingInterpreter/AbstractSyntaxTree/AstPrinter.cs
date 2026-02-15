namespace CraftingInterpreter.AbstractSyntaxTree;

public class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expression) 
    {
        var result = expression.Accept(this);
        
        return result ?? "";
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

    private string Parenthesize(string name, params Expr[] arguments) =>
        $"({name} {string.Join(" ", arguments.Select(e => e.Accept(this)))})";
}