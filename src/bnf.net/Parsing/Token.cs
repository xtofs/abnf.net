using System.Globalization;
namespace Bnf.Parsing;

public sealed class Token(TokenKind kind, string value, int line, int column)
{
    /// <summary>
    /// Decodes the value of a Literal, NumberVal (percent notation), or ValueRange token to a string.
    /// Throws if not a supported kind.
    /// </summary>
    public string StringValue()
    {
    if (Kind == TokenKind.CharVal || Kind == TokenKind.CaseSensitiveCharVal)
        {
            // Remove surrounding quotes (single or double)
            if (Value.Length >= 2 && (Value[0] == '"' || Value[0] == '\''))
                return Value.Substring(1, Value.Length - 2);
            return Value;
        }
    if (Kind == TokenKind.NumVal)
        {
            // Only support %x... (hex) for now
            if (Value.Length < 3 || (Value[0] != '%' || char.ToLowerInvariant(Value[1]) != 'x'))
                throw new NotSupportedException($"Only %x... notation is supported: {Value}");
            var bytes = Value.Substring(2).Split('.');
            var chars = new char[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i].Length == 0)
                    throw new FormatException($"Empty byte in percent notation: {Value}");
                var code = int.Parse(bytes[i], NumberStyles.HexNumber);
                chars[i] = (char)code;
            }
            return new string(chars);
        }
        if (Kind == TokenKind.ValueRange)
        {
            // Return the raw value for now; decoding a range is not meaningful as a string
            return Value;
        }
    throw new InvalidOperationException($"Token is not a CharVal, NumVal, or ValueRange: {Kind}");
    }
    public TokenKind Kind { get; } = kind;
    public string Value { get; } = value;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString() => $"{Kind} '{Value}' @ {Line}:{Column}";
}
