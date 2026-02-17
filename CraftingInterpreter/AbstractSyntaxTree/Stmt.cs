using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.AbstractSyntaxTree;

public abstract class Stmt
{
    public interface IVisitor<out T>
    {
        T? VisitBlockStmt(Block stmt);
        T? VisitClassStmt(Class stmt);
        T? VisitExpressionStmt(Expression stmt);
        T? VisitFunctionStmt(Function stmt);
        T? VisitPrintStmt(Print stmt);
        T? VisitVarStmt(Var stmt);
        T? VisitIfStmt(If stmt);
        T? VisitWhileStmt(While stmt);
        T? VisitReturnStmt(Return stmt);
        T? VisitBreakStmt(Break stmt);
        T? VisitContinueStmt(Continue stmt);
    }

    public class Block(List<Stmt> @statements) : Stmt
    {
        public List<Stmt> Statements { get; } = @statements;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitBlockStmt(this);
    }

    public class Class(Token @name, List<Stmt.Function> @methods) : Stmt
    {
        public Token Name { get; } = @name;
        public List<Stmt.Function> Methods { get; } = @methods;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitClassStmt(this);
    }

    public class Expression(Expr @expr) : Stmt
    {
        public Expr Expr { get; } = @expr;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitExpressionStmt(this);
    }

    public class Function(Token @name, List<Token> @params, List<Stmt> @body) : Stmt
    {
        public Token Name { get; } = @name;
        public List<Token> Params { get; } = @params;
        public List<Stmt> Body { get; } = @body;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitFunctionStmt(this);
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

    public class If(Expr @condition, Stmt @thenBranch, Stmt? @elseBranch) : Stmt
    {
        public Expr Condition { get; } = @condition;
        public Stmt ThenBranch { get; } = @thenBranch;
        public Stmt? ElseBranch { get; } = @elseBranch;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitIfStmt(this);
    }

    public class While(Expr @condition, Stmt @body, Expr? @increment = null) : Stmt
    {
        public Expr Condition { get; } = @condition;
        public Stmt Body { get; } = @body;
        public Expr? Increment { get; } = @increment;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitWhileStmt(this);
    }

    public class Return(Token @keyword, Expr? @value) : Stmt
    {
        public Token Keyword { get; } = @keyword;
        public Expr? Value { get; } = @value;

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitReturnStmt(this);
    }

    public class Break : Stmt
    {

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitBreakStmt(this);
    }

    public class Continue : Stmt
    {

        public override T? Accept<T>(IVisitor<T> visitor) where T : default => visitor.VisitContinueStmt(this);
    }

    public abstract T? Accept<T>(IVisitor<T> visitor);
}
