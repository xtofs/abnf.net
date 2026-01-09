using System.Globalization;
using System.Text.RegularExpressions;

namespace Abnf.Parsing;

/// <summary>
/// Converts ABNF AST nodes into Grammar patterns for validation.
/// </summary>
public static class AstToGrammarConverter
{
    /// <summary>
    /// Converts an AST RuleList into a Grammar.
    /// </summary>
    public static Grammar ToGrammar(AstNode.RuleList ruleList)
    {
        var grammarRules = ruleList.Rules.Select(ConvertRule).ToList();
        return new Grammar(grammarRules);
    }

    private static GrammarRule ConvertRule(AstNode.Rule rule)
    {
        var pattern = ConvertExpression(rule.Expr);
        return new GrammarRule(rule.Name, pattern);
    }

    private static Pattern ConvertExpression(AstNode.Expression expr)
    {
        return expr switch
        {
            AstNode.Expression.Alternation alt => ConvertAlternation(alt),
            AstNode.Expression.Concatenation concat => ConvertConcatenation(concat),
            AstNode.Expression.Repetition rep => ConvertRepetition(rep),
            AstNode.Expression.Group group => ConvertExpression(group.Inner),
            AstNode.Expression.Option opt => new Pattern.Repetition(ConvertExpression(opt.Inner), min: 0, max: 1),
            AstNode.Expression.Literal lit => new Pattern.Terminal(lit.Value, lit.IsCaseSensitive),
            AstNode.Expression.NumberVal num => ConvertNumberVal(num.Value),
            AstNode.Expression.RuleRef ruleRef => new Pattern.RuleReference(ruleRef.Name),
            AstNode.Expression.ProseVal prose => throw new NotSupportedException($"Prose values are not supported for validation: '{prose.Value}'"),
            _ => throw new ArgumentException($"Unknown expression type: {expr.GetType().Name}")
        };
    }

    private static Pattern ConvertAlternation(AstNode.Expression.Alternation alternation)
    {
        var alternatives = alternation.Options.Select(ConvertExpression).ToList();
        return new Pattern.Alternation(alternatives);
    }

    private static Pattern ConvertConcatenation(AstNode.Expression.Concatenation concatenation)
    {
        var elements = concatenation.Elements.Select(ConvertExpression).ToList();
        return new Pattern.Sequence(elements);
    }

    private static Pattern ConvertRepetition(AstNode.Expression.Repetition repetition)
    {
        var element = ConvertExpression(repetition.Element);
        return new Pattern.Repetition(element, repetition.Min, repetition.Max);
    }

    /// <summary>
    /// Converts ABNF numeric values (e.g., %x41, %d65, %x41-5A) into patterns.
    /// Supports:
    /// - Single values: %x41 (hex) or %d65 (decimal) -> matches that character
    /// - Ranges: %x41-5A -> matches characters in that range
    /// - Concatenation: %x41.42.43 -> matches those characters in sequence
    /// </summary>
    private static Pattern ConvertNumberVal(string value)
    {
        // Remove % prefix and determine base
        if (!value.StartsWith("%"))
        {
            throw new ArgumentException($"Invalid number value format: {value}");
        }

        var rest = value.Substring(1);
        int numberBase;

        if (rest.StartsWith("x", StringComparison.OrdinalIgnoreCase))
        {
            numberBase = 16;
            rest = rest.Substring(1);
        }
        else if (rest.StartsWith("d", StringComparison.OrdinalIgnoreCase))
        {
            numberBase = 10;
            rest = rest.Substring(1);
        }
        else if (rest.StartsWith("b", StringComparison.OrdinalIgnoreCase))
        {
            numberBase = 2;
            rest = rest.Substring(1);
        }
        else
        {
            throw new ArgumentException($"Invalid number value format: {value}");
        }

        // Check for range (e.g., 41-5A)
        if (rest.Contains("-"))
        {
            var parts = rest.Split('-');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid range format: {value}");
            }

            var min = Convert.ToInt32(parts[0], numberBase);
            var max = Convert.ToInt32(parts[1], numberBase);
            return new Pattern.CharacterRange(min, max);
        }

        // Check for concatenation (e.g., 41.42.43)
        if (rest.Contains("."))
        {
            var parts = rest.Split('.');
            var chars = parts.Select(p => Convert.ToInt32(p, numberBase)).ToList();
            var patterns = chars.Select(c => (Pattern)new Pattern.CharacterValue(c)).ToList();
            return new Pattern.Sequence(patterns);
        }

        // Single value
        var charValue = Convert.ToInt32(rest, numberBase);
        return new Pattern.CharacterValue(charValue);
    }
}
