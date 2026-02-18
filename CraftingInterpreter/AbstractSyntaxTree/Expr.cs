using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Expr
{
    public interface IVisitor<out T>
    {
        T? VisitAssignExpr(Assign expr);
        T? VisitBinaryExpr(Binary expr);
        T? VisitCallExpr(Call expr);
        T? VisitGetExpr(Get expr);
        T? VisitGroupingExpr(Grouping expr);
        T? VisitLiteralExpr(Literal expr);
        T? VisitLogicalExpr(Logical expr);
        T? VisitThisExpr(This expr);
        T? VisitSuperExpr(Super expr);
        T? VisitSetExpr(Set expr);
        T? VisitUnaryExpr(Unary expr);
        T? VisitTernaryExpr(Ternary expr);
        T? VisitCommaExpr(Comma expr);
        T? VisitLambdaExpr(Lambda expr);
        T? VisitVariableExpr(Variable expr);
    }

    public class Assign(Token @name, Expr? @value) : Expr
    {
        public Token Name { get; } = @name;
        public Expr? Value { get; } = @value;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitAssignExpr(this);
    }

    public class Binary(Expr @left, Token @operator, Expr @right) : Expr
    {
        public Expr Left { get; } = @left;
        public Token Operator { get; } = @operator;
        public Expr Right { get; } = @right;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitBinaryExpr(this);
    }

    public class Call(Expr @callee, Token @paren, List<Expr> @arguments) : Expr
    {
        public Expr Callee { get; } = @callee;
        public Token Paren { get; } = @paren;
        public List<Expr> Arguments { get; } = @arguments;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitCallExpr(this);
    }

    public class Get(Expr @object, Token @name) : Expr
    {
        public Expr Object { get; } = @object;
        public Token Name { get; } = @name;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitGetExpr(this);
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

    public class Logical(Expr @left, Token @operator, Expr @right) : Expr
    {
        public Expr Left { get; } = @left;
        public Token Operator { get; } = @operator;
        public Expr Right { get; } = @right;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitLogicalExpr(this);
    }

    public class This(Token @keyword) : Expr
    {
        public Token Keyword { get; } = @keyword;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitThisExpr(this);
    }

    public class Super(Token @keyword, Token @method) : Expr
    {
        public Token Keyword { get; } = @keyword;
        public Token Method { get; } = @method;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitSuperExpr(this);
    }

    public class Set(Expr @object, Token @name, Expr? @value) : Expr
    {
        public Expr Object { get; } = @object;
        public Token Name { get; } = @name;
        public Expr? Value { get; } = @value;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitSetExpr(this);
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

    public class Lambda(List<Token> @params, List<Stmt> @body) : Expr
    {
        public List<Token> Params { get; } = @params;
        public List<Stmt> Body { get; } = @body;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitLambdaExpr(this);
    }

    public class Variable(Token @name) : Expr
    {
        public Token Name { get; } = @name;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitVariableExpr(this);
    }

    public abstract T? Accept<T>(IVisitor<T> visitor);
}
