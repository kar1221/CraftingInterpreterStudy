using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.LoxConsole;
using CraftingInterpreter.Parsing.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Parsing;

/* Expression */
/*
 * Expression -> Assignment
 * Assignment -> IDENTIFIER "=" Assignment | Comma
 * Comma -> Conditional ( "," Conditional )*
 * Conditional -> Equality ( "?" Expression ":" Conditional )*
 * Equality -> Comparison ( ( "!=" | "==" ) Comparison)*
 * Comparison -> Term ( ( ">" | ">=" | "<" | "<=" ) Term)*
 * Term -> Factor ( ( "-" | "+") Factor )*
 * Factor -> Unary ( ( "/" | "*" ) Unary )*
 * Unary -> ( "!" | "-" ) Unary | Primary | ErrorBinary
 * ErrorBinary -> ( "," | "?" | "==" | "!=" | "<" | "<=" | ">" | ">=" | "+" | "*" | "/" ) Expression
 * Primary -> NUMBER | STRING | "true" | "false" | "nil" | "{" Expression "} | IDENTIFIER"
 */
/* Statement */
/*
 * Program -> Declaration* EOF
 * Declaration -> VarDeclaration | Statement
 * Statement -> ExprStatement | PrintStatement
 * VarDeclaration -> "var" IDENTIFIER ( "=" Expression )? ";"
 * Block -> "{" Declaration* "}"
 */

public class Parser(List<Token> tokens)
{
    private int _current;

    public Expr? ParseSingle()
    {
        try
        {
            return Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();

        while (!IsAtEnd())
            statements.Add(Declaration()!);

        return statements;
    }

    #region Expression

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Comma();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable v)
            {
                var name = v.Name;
                return new Expr.Assign(name, value);
            }

            throw Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Comma()
    {
        var expr = Conditional();

        while (Match(TokenType.Comma))
        {
            var right = Conditional();
            expr = new Expr.Comma(expr, right);
        }

        return expr;
    }

    private Expr Conditional()
    {
        var expr = Equality();

        if (Match(TokenType.Question))
        {
            var thenBranch = Expression();
            Consume(TokenType.Colon, "Expect ':' after then branch of ternary expression.");
            var elseBranch = Conditional();

            expr = new Expr.Ternary(expr, thenBranch, elseBranch);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var operatorToken = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var operatorToken = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var operatorToken = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Slash, TokenType.Star))
        {
            var operatorToken = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            return new Expr.Unary(Previous(), Unary());
        }

        if (!Match(TokenType.Comma, TokenType.Question, TokenType.EqualEqual, TokenType.BangEqual, TokenType.Less,
                TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Plus, TokenType.Minus,
                TokenType.Slash, TokenType.Star)) return Primary();

        var operatorToken = Previous();
        Unary();
        throw Error(operatorToken, "Binary operator missing left-hand operand");
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.Identifier))
            return new Expr.Variable(Previous());
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expected ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), $"Unexpected Token {Peek()}");
    }

    #endregion

    private Token Consume(TokenType type, string message)
    {
        return Check(type) ? Advance() : throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Eof)
                return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }

    #region Statement

    private Stmt Statement()
    {
        if (Match(TokenType.Print))
            return PrintStatement();

        if (Match(TokenType.LeftBrace))
            return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Var))
                return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");

        Expr? initializer = null;

        if (Match(TokenType.Equal))
            initializer = Expression();

        Consume(TokenType.SemiColon, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            var stmt = Declaration();

            if (stmt == null)
                continue;

            statements.Add(stmt);
        }

        Consume(TokenType.RightBrace, "Expect '}' after block");

        return statements;
    }

    #endregion


    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check)) return false;

        Advance();
        return true;
    }


    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.Eof;


    private Token Peek() => tokens[_current];

    private Token Previous() => tokens[_current - 1];
}