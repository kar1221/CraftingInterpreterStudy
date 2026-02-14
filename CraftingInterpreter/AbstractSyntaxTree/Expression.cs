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

    public abstract T Accept<T>(IVisitor<T> visitor);
}
