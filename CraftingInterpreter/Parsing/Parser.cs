using CraftingInterpreter.AbstractSyntaxTree;
using CraftingInterpreter.LoxConsole;
using CraftingInterpreter.Parsing.Exceptions;
using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Parsing;
/*
 * Expression -> Comma
 * Comma -> Conditional ( "," Conditional )*
 * Conditional -> Equality ( "?" Expression ":" Conditional )*
 * Equality -> Comparison ( ( "!=" | "==" ) Comparison)*
 * Comparison -> Term ( ( ">" | ">=" | "<" | "<=" ) Term)*
 * Term -> Factor ( ( "-" | "+") Factor )*
 * Factor -> Unary ( ( "/" | "*" ) Unary )*
 * Unary -> ( "!" | "-" ) Unary | Primary | ErrorBinary
 * ErrorBinary -> ( "," | "?" | "==" | "!=" | "<" | "<=" | ">" | ">=" | "+" | "*" | "/" ) Expression
 * Primary -> NUMBER | STRING | "true" | "false" | "nil" | "{" Expression "}"
 */

public class Parser(List<Token> tokens)
{
    private int _current;

    public Expression? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseException e)
        {
            return null;
        }
    }

    private Expression Expression()
    {
        return Comma();
    }

    private Expression Comma()
    {
        var expr = Conditional();

        while (Match(TokenType.Comma))
        {
            var right = Conditional();
            expr = new Expression.Comma(expr, right);
        }

        return expr;
    }

    private Expression Conditional()
    {
        var expr = Equality();

        if (Match(TokenType.Question))
        {
            var thenBranch = Expression();
            Consume(TokenType.Colon, "Expect ':' after then branch of ternary expression.");
            var elseBranch = Conditional();

            expr = new Expression.Ternary(expr, thenBranch, elseBranch);
        }

        return expr;
    }

    private Expression Equality()
    {
        var expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var operatorToken = Previous();
            var right = Comparison();
            expr = new Expression.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expression Comparison()
    {
        var expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var operatorToken = Previous();
            var right = Term();
            expr = new Expression.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expression Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var operatorToken = Previous();
            var right = Factor();
            expr = new Expression.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expression Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Slash, TokenType.Star))
        {
            var operatorToken = Previous();
            var right = Unary();
            expr = new Expression.Binary(expr, operatorToken, right);
        }

        return expr;
    }

    private Expression Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            return new Expression.Unary(Previous(), Unary());
        }

        if (!Match(TokenType.Comma, TokenType.Question, TokenType.EqualEqual, TokenType.BangEqual, TokenType.Less,
                TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual, TokenType.Plus, TokenType.Minus,
                TokenType.Slash, TokenType.Star)) return Primary();
        
        var operatorToken = Previous();
        Unary();
        throw Error(operatorToken, "Binary operating missing left-hand operand");
    }

    private Expression Primary()
    {
        if (Match(TokenType.False)) return new Expression.Literal(false);
        if (Match(TokenType.True)) return new Expression.Literal(true);
        if (Match(TokenType.Nil)) return new Expression.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expression.Literal(Previous().Literal);
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expected ')' after expression.");
            return new Expression.Grouping(expr);
        }

        throw Error(Peek(), $"Unexpected Token {Peek()}");
    }

    private Token Consume(TokenType type, string message)
    {
        return Check(type) ? Advance() : throw Error(Peek(), message);
    }

    private static ParseException Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseException();
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