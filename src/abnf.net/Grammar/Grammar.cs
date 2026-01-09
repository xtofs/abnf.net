namespace Abnf;

/// <summary>
/// Represents a complete grammar with multiple rules.
/// Provides methods to validate strings against the grammar.
/// </summary>
public sealed class Grammar(IEnumerable<GrammarRule> rules)
{
    private readonly Dictionary<string, GrammarRule> _rules = rules.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<GrammarRule> Rules => _rules.Values;

    /// <summary>
    /// Validates an input string against a specific rule in the grammar.
    /// Returns a structured ValidationResult with error position and message.
    /// </summary>
    /// <param name="input">The input string to validate</param>
    /// <param name="startRuleName">The name of the rule to start validation from</param>
    /// <returns>ValidationResult with success status, error position (0-based), and message</returns>
    public ValidationResult Validate(string input, string startRuleName)
    {
        if (!_rules.TryGetValue(startRuleName, out var startRule))
        {
            return ValidationResult.Failure(0, $"Start rule '{startRuleName}' not found in grammar");
        }

        var context = new GrammarContext(_rules.Values);
        var matchResult = startRule.Pattern.Match(input, 0, context);

        if (!matchResult.IsSuccess)
        {
            // Report the furthest position reached (0-based)
            return ValidationResult.Failure(matchResult.FurthestPosition, matchResult.FurthestErrorMessage);
        }

        // Check if the entire input was consumed
        if (matchResult.Position < input.Length)
        {
            return ValidationResult.Failure(
                matchResult.Position,
                $"Matched successfully but {input.Length - matchResult.Position} character(s) remaining: '{input.Substring(matchResult.Position)}'");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Attempts to validate an input string against a specific rule in the grammar.
    /// Returns true if validation succeeds, false otherwise.
    /// Note: Positions are 1-based for backwards compatibility.
    /// Consider using Validate() for structured results with 0-based positions.
    /// </summary>
    /// <param name="input">The input string to validate</param>
    /// <param name="startRuleName">The name of the rule to start validation from</param>
    /// <param name="errorPosition">When validation fails, contains the 1-based position where the error occurred</param>
    /// <param name="errorMessage">When validation fails, contains the error message</param>
    /// <returns>True if validation succeeds, false otherwise</returns>
    public bool TryValidate(string input, string startRuleName, out int errorPosition, out string errorMessage)
    {
        var result = Validate(input, startRuleName);
        
        if (result.IsSuccess)
        {
            errorPosition = 0;
            errorMessage = string.Empty;
            return true;
        }

        // Convert to 1-based for backwards compatibility
        errorPosition = result.ErrorPosition + 1;
        errorMessage = result.ErrorMessage;
        return false;
    }

    /// <summary>
    /// Gets a rule by name (case-insensitive).
    /// </summary>
    public bool TryGetRule(string name, out GrammarRule rule)
    {
        return _rules.TryGetValue(name, out rule!);
    }
}
