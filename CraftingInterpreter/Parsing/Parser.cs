using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.LoxConsole;
using CraftingInterpreter.Parsing.Errors;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Parsing;

/* Expression */
/*
 * Expression -> Comma ;
 * Comma -> Assignment ( "," Assignment )* ;
 * Assignment -> ( IDENTIFIER ( "=" | "+=" | "-=" | "*=" | "/=" ) Assignment ) | Conditional ;
 * Conditional -> LogicOr ( "?" Expression ":" Conditional )? ;
 * LogicOr -> LogicAnd ( "or" LogicAnd )* ;
 * LogicAnd -> Equality ( "and" Equality )* ;
 * Equality -> Comparison ( ( "!=" | "==" ) Comparison)* ;
 * Comparison -> Term ( ( ">" | ">=" | "<" | "<=" ) Term)* ;
 * Term -> Factor ( ( "-" | "+") Factor )* ;
 * Factor -> Unary ( ( "/" | "*" ) Unary )* ;
 * Unary -> ( "!" | "-" | ) Unary | Primary | ErrorBinary ;
 * ErrorBinary -> ( "," | "?" | "==" | "!=" | "<" | "<=" | ">" | ">=" | "+" | "*" | "/" ) Expression ;
 * Primary -> NUMBER | STRING | "true" | "false" | "nil" | "{" Expression "}" | IDENTIFIER ;
 */
/* Statement */
/*
 * Program -> Declaration* EOF ;
 * Declaration -> VarDeclaration | Statement ;
 * VarDeclaration -> "var" IDENTIFIER ( "=" Expression )? ";" ;
 * Statement -> ExprStatement | PrintStatement | Block | IfStatement | WhileStatement | BreakStatement | ContinueStatement;
 * ExprStatement -> Expression ";" ;
 * PrintStatement -> "print" Expression ";" ;
 * Block -> "{" Declaration* "}" ;
 * IfStatement -> "if" "(" Expression ")" Statement ( "else" statement )? ;
 * WhileStatement -> "while" "(" Expression ")" statement ;
 * ForStatement -> "for" "(" ( VarDeclaration | ExprStatement | ";" ) Expression? ";" Expression? ")" Statement;
 * BreakStatement -> "break" ";" ;
 * ContinueStatement -> "continue" ";" ;
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
        return Comma();
    }

    private Expr Comma()
    {
        var expr = Assignment();

        while (Match(TokenType.Comma))
        {
            var right = Assignment();
            expr = new Expr.Comma(expr, right);
        }

        return expr;
    }

    private Expr Assignment()
    {
        var expr = Conditional();

        if (!Match(TokenType.Equal, TokenType.PlusEqual, TokenType.MinusEqual, TokenType.StarEqual,
                TokenType.SlashEqual)) 
            return expr;
        
        var op = Previous();
        var value = Assignment();

        if (expr is not Expr.Variable v)
            throw Error(op, "Invalid assignment target.");

        var name = v.Name;

        if (op.Type == TokenType.Equal)
        {
            return new Expr.Assign(name, value);
        }
        
        var binaryType = op.Type switch
        {
            TokenType.PlusEqual => TokenType.Plus,
            TokenType.MinusEqual => TokenType.Minus,
            TokenType.StarEqual => TokenType.Star,
            TokenType.SlashEqual => TokenType.Slash,
            _ => throw Error(op, "Unknown compound assignment operator")
        };

        var binaryToken = new Token(binaryType, op.Lexeme[..1], null, op.Line);

        var result = new Expr.Binary(expr, binaryToken, value);
        return new Expr.Assign(name, result);
    }

    private Expr Conditional()
    {
        var expr = LogicOr();

        if (!Match(TokenType.Question))
            return expr;

        var thenBranch = Expression();

        Consume(TokenType.Colon, "Expect ':' after then branch of ternary expression.");
        var elseBranch = Conditional();

        expr = new Expr.Ternary(expr, thenBranch, elseBranch);

        return expr;
    }

    private Expr LogicOr()
    {
        var expr = LogicAnd();

        while (Match(TokenType.Or))
        {
            var @operator = Previous();
            var right = LogicAnd();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr LogicAnd()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var @operator = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, @operator, right);
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
        if (Match(TokenType.Break))
            return BreakStatement();

        if (Match(TokenType.Continue))
            return ContinueStatement();

        if (Match(TokenType.For))
            return ForStatement();

        if (Match(TokenType.While))
            return WhileStatement();

        if (Match(TokenType.If))
            return IfStatement();

        if (Match(TokenType.Print))
            return PrintStatement();

        if (Match(TokenType.LeftBrace))
            return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt.If IfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition");

        var thenBranch = Statement();
        Stmt? elseBranch = null;

        if (Match(TokenType.Else))
            elseBranch = Statement();

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt.While WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' while condition");

        var body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

        Stmt? initializer;

        if (Match(TokenType.SemiColon))
            initializer = null;
        else if (Match(TokenType.Var))
            initializer = VarDeclaration();
        else
            initializer = ExpressionStatement();

        Expr? condition = null;

        if (!Check(TokenType.SemiColon))
            condition = Expression();

        Consume(TokenType.SemiColon, "Expect ';' after loop condition.");

        Expr? increment = null;

        if (!Check(TokenType.RightParen))
            increment = Expression();

        Consume(TokenType.RightParen, "Expect ')' after for clause");

        var body = Statement();

        if (increment != null)
        {
            body = new Stmt.Block([body, new Stmt.ForIncrement(increment)]);
        }

        condition ??= new Expr.Literal(true);

        body = new Stmt.While(condition, body);

        if (initializer != null)
            body = new Stmt.Block([initializer, body]);

        return body;
    }

    private Stmt.Print PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt.Expression ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt.Break BreakStatement()
    {
        Consume(TokenType.SemiColon, "Expect ';' after 'break'.");
        return new Stmt.Break();
    }

    private Stmt.Continue ContinueStatement()
    {
        Consume(TokenType.SemiColon, "Expect ';' after 'continue'.");
        return new Stmt.Continue();
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

    private Stmt.Var VarDeclaration()
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