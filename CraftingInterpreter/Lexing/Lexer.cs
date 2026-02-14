using CraftingInterpreter.TokenModels;

namespace CraftingInterpreter.Lexing;

public class Lexer(string source)
{
    private readonly List<Token> _tokens = [];

    private int _start;

    private int _current;

    private int _line = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["and"] = TokenType.And,
        ["class"] = TokenType.Class,
        ["else"] = TokenType.Else,
        ["false"] = TokenType.False,
        ["for"] = TokenType.For,
        ["fun"] = TokenType.Fun,
        ["if"] = TokenType.If,
        ["nil"] = TokenType.Nil,
        ["or"] = TokenType.Or,
        ["print"] = TokenType.Print,
        ["return"] = TokenType.Return,
        ["super"] = TokenType.Super,
        ["this"] = TokenType.This,
        ["true"] = TokenType.True,
        ["var"] = TokenType.Var,
        ["while"] = TokenType.While,
    };

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();

        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.SemiColon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else if (Match('*'))
                {
                    SkipBlockComments();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            case '"':
                ReadString();
                break;
            default:
                if (IsDigit(c))
                {
                    ReadNumber();
                }
                else if (IsAlpha(c))
                {
                    ReadIdentifier();
                }
                else
                {
                    Lox.Error(_line, "Unexpected Character.");
                }

                break;
        }
    }

    private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

    private void ReadNumber()
    {
        while (IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();

            while (IsDigit(Peek()))
                Advance();
        }

        var result = source.Substring(_start, _current - _start);
        AddToken(TokenType.Number, double.Parse(result));
    }

    private void ReadIdentifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();

        var text = source.Substring(_start, _current - _start);

        var type = Keywords.GetValueOrDefault(text, TokenType.Identifier);

        AddToken(type);
    }

    private void ReadString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                _line++;

            Advance();
        }

        if (IsAtEnd())
        {
            Lox.Error(_line, "Unexpected end of string.");
            return;
        }

        Advance();

        var value = source.Substring(_start + 1, _current - _start - 1 - 1);
        AddToken(TokenType.String, value);
    }

    private void SkipBlockComments()
    {
        var depth = 1;
        
        while (!IsAtEnd() && depth > 0)
        {
            var c = Advance();

            switch (c)
            {
                case '\n':
                    _line++;
                    break;
                case '/' when Peek() == '*':
                    Advance(); 
                    depth++;
                    break;
                case '*' when Peek() == '/':
                    Advance();
                    depth--;
                    break;
            }
        }

        if(depth > 0)
            Lox.Error(_line, "Unexpected end of block comments.");
    }

    private char PeekNext()
    {
        return _current + 1 >= source.Length ? '\0' : source[_current + 1];
    }


    private bool Match(char expected)
    {
        if (IsAtEnd())
            return false;

        if (source[_current] != expected)
            return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : source[_current];
    }

    private char Advance()
    {
        _current++;
        return source[_current - 1];
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        var text = source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool IsAtEnd()
    {
        return _current >= source.Length;
    }
}