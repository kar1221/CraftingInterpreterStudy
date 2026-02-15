using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Stmt
{
    public interface IVisitor<out T>
    {
        T? VisitBlockStmt(Block stmt);
        T? VisitExpressionStmt(Expression stmt);
        T? VisitPrintStmt(Print stmt);
        T? VisitVarStmt(Var stmt);
    }

    public class Block(List<Stmt> @statements) : Stmt
    {
        public List<Stmt> Statements { get; } = @statements;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitBlockStmt(this);
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

    public class Var(Token @name, Expr? @initializer) : Stmt
    {
        public Token Name { get; } = @name;
        public Expr? Initializer { get; } = @initializer;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitVarStmt(this);
    }

    public abstract T? Accept<T>(IVisitor<T> visitor);
}
