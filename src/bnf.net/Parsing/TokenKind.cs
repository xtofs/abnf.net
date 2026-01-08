namespace Bnf.Parsing;

public enum TokenKind
{
    Repeat, // e.g. 1*2, 1*, *2
    Integer, // e.g. 1, 42
    Whitespace,
    Comment,
    CRLF,
    RuleName,
    CharVal, // double quoted string (case-insensitive, RFC 5234)
    CaseSensitiveCharVal, // single quoted string (case-sensitive, RFC 7405)
    ProseVal, // <...>
    NumVal, // percent notation, e.g. %x0D.0A
    ValueRange, // value range notation, e.g. %x41-5A
    Equal,
    Slash,
    Star,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    OpenAngle,
    CloseAngle,
    Percent,
    OtherSymbol,
    EndOfInput
}
