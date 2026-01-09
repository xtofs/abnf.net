using System.Globalization;
namespace Abnf.Parsing;

public sealed class Token(TokenKind kind, string value, int line, int column)
{
    
    public TokenKind Kind { get; } = kind;
    public string Value { get; } = value;
    public int Line { get; } = line;
    public int Column { get; } = column;


    /// <summary>
    /// Decodes the value of a Literal, NumberVal (percent notation), or ValueRange token to a string.
    /// Throws if not a supported kind.
    /// </summary>
    /// <throws cref="InvalidOperationException">if Kind is not Literal, or NumberVal </throws>
    /// 
    public string GetStringValue()
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

    /// <summary>
    /// Decodes the value of a Repeat token and give the repetition bounds (e.g., "*", "1*", "*5", "1*3").
    /// Returns (min, max) where null for max means unbounded.
    /// Throws if not a Repeat token.
    /// </summary>
    /// <throws cref="InvalidOperationException"></throws>
    public (int? Min, int? Max) GetRepetitionBounds()
    {
        if (Kind != TokenKind.Repeat)
            throw new InvalidOperationException($"Token is not a Repeat: {Kind}");

        var parts = Value.Split('*');
        var min = string.IsNullOrEmpty(parts[0]) ? (int?)null : int.Parse(parts[0]);
        var max = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) : (int?)null;
        return (min, max);
    }

    public override string ToString() => $"{Kind} '{Value}' @ {Line}:{Column}";
}
