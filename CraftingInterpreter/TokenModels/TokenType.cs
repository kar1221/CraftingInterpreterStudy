namespace CraftingInterpreter.TokenModels;

public enum TokenType
{
    // Single Character Tokens
    LeftParen, // (
    RightParen, // )
    LeftBrace, // {
    RightBrace, // }
    Comma, // ,
    Dot, // .
    Minus, // -
    Plus, // +
    SemiColon, // ;
    Slash, // /
    Star, // *
    Question, // ?
    Colon, // :

    // One or two character tokens
    Bang, // !
    BangEqual, // !=
    Equal, // =
    EqualEqual, // ==
    Greater, // >
    GreaterEqual, // >=
    Less, // <
    LessEqual, // <=
    PlusEqual, // +=
    MinusEqual, // -=
    StarEqual, // *=
    SlashEqual, // /=

    // Literals
    Identifier,
    String,
    Number,

    // Keywords
    And, // and
    Class, // class
    Else, // else
    False, // false
    Fun, // fun
    For, // for
    If, // if
    Nil, // nil
    Or, // or
    Print, // print
    Return, // return
    Super, // super
    This, // this
    True, // true
    Var, // var
    While, // while
    Break, // break
    Continue, // continue

    Eof,
}