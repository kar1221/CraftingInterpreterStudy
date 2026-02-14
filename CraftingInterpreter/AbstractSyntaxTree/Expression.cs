using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Expression
{
    public interface IVisitor<out T>
    {
        T VisitBinaryExpression(Binary expression);
        T VisitGroupingExpression(Grouping expression);
        T VisitLiteralExpression(Literal expression);
        T VisitUnaryExpression(Unary expression);
        T VisitTernaryExpression(Ternary expression);
        T VisitCommaExpression(Comma expression);
    }

    public class Binary(Expression @left, Token @operator, Expression @right) : Expression
    {
        public Expression Left { get; } = @left;
        public Token Operator { get; } = @operator;
        public Expression Right { get; } = @right;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
    }

    public class Grouping(Expression @expression) : Expression
    {
        public Expression Expression { get; } = @expression;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpression(this);
    }

    public class Literal(object? @value) : Expression
    {
        public object? Value { get; } = @value;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpression(this);
    }

    public class Unary(Token @operator, Expression @right) : Expression
    {
        public Token Operator { get; } = @operator;
        public Expression Right { get; } = @right;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
    }

    public class Ternary(Expression @condition, Expression @thenBranch, Expression @elseBranch) : Expression
    {
        public Expression Condition { get; } = @condition;
        public Expression ThenBranch { get; } = @thenBranch;
        public Expression ElseBranch { get; } = @elseBranch;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitTernaryExpression(this);
    }

    public class Comma(Expression @evaluate, Expression @return) : Expression
    {
        public Expression Evaluate { get; } = @evaluate;
        public Expression Return { get; } = @return;

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCommaExpression(this);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}
