using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Stmt
{
    public interface IVisitor<out T>
    {
        T? VisitExpressionStmt(Expression stmt);
        T? VisitPrintStmt(Print stmt);
    }

    public class Expression(Expr @expr) : Stmt
    {
        public Expr Expr { get; } = @expr;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitExpressionStmt(this);
    }

    public class Print(Expr @expr) : Stmt
    {
        public Expr Expr { get; } = @expr;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitPrintStmt(this);
    }

    public abstract T? Accept<T>(IVisitor<T> visitor);
}
