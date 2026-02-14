using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public class AstPrinter : Expression.IVisitor<string>
{
    public string Print(Expression expression) => expression.Accept(this);

    public string VisitBinaryExpression(Expression.Binary expression)
    {
        return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
    }

    public string VisitGroupingExpression(Expression.Grouping expression)
    {
        return Parenthesize("group", expression.Expression);
    }

    public string VisitLiteralExpression(Expression.Literal expression)
    {
        if (expression.Value == null)
            return "nil";

        return expression.Value.ToString() ?? "";
    }

    public string VisitUnaryExpression(Expression.Unary expression)
    {
        return Parenthesize(expression.Operator.Lexeme, expression.Right);
    }

    private string Parenthesize(string name, params Expression[] arguments) =>
        $"({name} {string.Join(" ", arguments.Select(e => e.Accept(this)))})";

}