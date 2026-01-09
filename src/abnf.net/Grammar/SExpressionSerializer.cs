using System.Text;

namespace Abnf;

/// <summary>
/// Serializes grammar rules to S-expression format for visualization and debugging.
/// </summary>
public static class SExpressionSerializer
{
    /// <summary>
    /// Converts a Grammar to S-expression format showing all rules.
    /// </summary>
    public static string ToSExpression(this Grammar grammar)
    {
        var sb = new StringBuilder();
        sb.AppendLine("(grammar");
        
        foreach (var rule in grammar.Rules)
        {
            sb.Append("  ");
            sb.AppendLine(rule.ToSExpression());
        }
        
        sb.Append(")");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a GrammarRule to S-expression format.
    /// </summary>
    public static string ToSExpression(this GrammarRule rule)
    {
        return $"(rule {rule.Name} {rule.Pattern.ToSExpression()})";
    }

    /// <summary>
    /// Converts a Pattern to S-expression format.
    /// </summary>
    public static string ToSExpression(this Pattern pattern)
    {
        return pattern switch
        {
            Pattern.Terminal terminal => 
                terminal.IsCaseSensitive 
                    ? $"(terminal-cs \"{Escape(terminal.Value)}\")"
                    : $"(terminal \"{Escape(terminal.Value)}\")",
            
            Pattern.CharacterValue charValue => 
                $"(char {FormatCharForSExpr(charValue.Value)})",
            
            Pattern.CharacterRange charRange => 
                $"(char-range {FormatCharForSExpr(charRange.MinValue)} {FormatCharForSExpr(charRange.MaxValue)})",
            
            Pattern.RuleReference ruleRef => 
                $"(ref {ruleRef.RuleName})",
            
            Pattern.Sequence sequence => 
                FormatList("seq", sequence.Elements),
            
            Pattern.Alternation alternation => 
                FormatList("alt", alternation.Alternatives),
            
            Pattern.Repetition repetition => 
                FormatRepetition(repetition),
            
            _ => $"(unknown {pattern.GetType().Name})"
        };
    }

    private static string FormatList(string name, IReadOnlyList<Pattern> elements)
    {
        if (elements.Count == 0)
            return $"({name})";
        
        if (elements.Count == 1)
            return elements[0].ToSExpression();
        
        var sb = new StringBuilder();
        sb.Append('(').Append(name);
        foreach (var element in elements)
        {
            sb.Append(' ').Append(element.ToSExpression());
        }
        sb.Append(')');
        return sb.ToString();
    }

    private static string FormatRepetition(Pattern.Repetition repetition)
    {
        var minStr = repetition.Min?.ToString() ?? "0";
        var maxStr = repetition.Max?.ToString() ?? "∞";
        
        return $"(repeat {minStr} {maxStr} {repetition.Element.ToSExpression()})";
    }

    private static string FormatCharForSExpr(int value)
    {
        // For printable ASCII, show both character and hex
        if (value >= 32 && value <= 126)
        {
            char c = (char)value;
            // Escape special characters
            return c switch
            {
                '"' => "0x22", // quote - use hex to avoid confusion
                '\\' => "0x5C", // backslash
                _ => $"'{c}'" // regular printable char
            };
        }
        
        // For non-printable, use hex notation
        return $"0x{value:X2}";
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
