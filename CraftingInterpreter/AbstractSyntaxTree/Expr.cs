using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Expr
{
    public interface IVisitor<out T>
    {
        T? VisitBinaryExpr(Binary expr);
        T? VisitGroupingExpr(Grouping expr);
        T? VisitLiteralExpr(Literal expr);
        T? VisitUnaryExpr(Unary expr);
        T? VisitTernaryExpr(Ternary expr);
        T? VisitCommaExpr(Comma expr);
    }

    public class Binary(Expr @left, Token @operator, Expr @right) : Expr
    {
        public Expr Left { get; } = @left;
        public Token Operator { get; } = @operator;
        public Expr Right { get; } = @right;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitBinaryExpr(this);
    }

    public class Grouping(Expr @expression) : Expr
    {
        public Expr Expression { get; } = @expression;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitGroupingExpr(this);
    }

    public class Literal(object? @value) : Expr
    {
        public object? Value { get; } = @value;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitLiteralExpr(this);
    }

    public class Unary(Token @operator, Expr @right) : Expr
    {
        public Token Operator { get; } = @operator;
        public Expr Right { get; } = @right;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitUnaryExpr(this);
    }

    public class Ternary(Expr @condition, Expr @thenBranch, Expr @elseBranch) : Expr
    {
        public Expr Condition { get; } = @condition;
        public Expr ThenBranch { get; } = @thenBranch;
        public Expr ElseBranch { get; } = @elseBranch;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitTernaryExpr(this);
    }

    public class Comma(Expr @left, Expr @right) : Expr
    {
        public Expr Left { get; } = @left;
        public Expr Right { get; } = @right;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitCommaExpr(this);
    }

    public abstract T? Accept<T>(IVisitor<T> visitor);
}
