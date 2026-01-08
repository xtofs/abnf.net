namespace Bnf.Grammar;

/// <summary>
/// Represents a complete grammar with multiple rules.
/// Provides methods to validate strings against the grammar.
/// </summary>
public sealed class Grammar
{
    private readonly Dictionary<string, GrammarRule> _rules;

    public IReadOnlyCollection<GrammarRule> Rules => _rules.Values;

    public Grammar(IEnumerable<GrammarRule> rules)
    {
        _rules = rules.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to validate an input string against a specific rule in the grammar.
    /// Returns true if validation succeeds, false otherwise.
    /// </summary>
    /// <param name="input">The input string to validate</param>
    /// <param name="startRuleName">The name of the rule to start validation from</param>
    /// <param name="errorPosition">When validation fails, contains the position where the error occurred</param>
    /// <param name="errorMessage">When validation fails, contains the error message</param>
    /// <returns>True if validation succeeds, false otherwise</returns>
    public bool TryValidate(string input, string startRuleName, out int errorPosition, out string errorMessage)
    {
        if (!_rules.TryGetValue(startRuleName, out var startRule))
        {
            errorPosition = 0;
            errorMessage = $"Start rule '{startRuleName}' not found in grammar";
            return false;
        }

        var context = new GrammarContext(_rules.Values);
        var matchResult = startRule.Pattern.Match(input, 0, context);

        if (!matchResult.IsSuccess)
        {
            errorPosition = matchResult.Position;
            errorMessage = matchResult.ErrorMessage;
            return false;
        }

        // Check if the entire input was consumed
        if (matchResult.Position < input.Length)
        {
            errorPosition = matchResult.Position;
            errorMessage = $"Matched successfully but {input.Length - matchResult.Position} character(s) remaining: '{input.Substring(matchResult.Position)}'";
            return false;
        }

        errorPosition = 0;
        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Gets a rule by name (case-insensitive).
    /// </summary>
    public bool TryGetRule(string name, out GrammarRule rule)
    {
        return _rules.TryGetValue(name, out rule!);
    }
}
