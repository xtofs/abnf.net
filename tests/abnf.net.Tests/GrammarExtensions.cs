namespace Abnf.Tests;

/// <summary>
/// Test helper extensions for Grammar validation assertions.
/// </summary>
public static class GrammarExtensions
{
    /// <summary>
    /// Asserts that the input is valid according to the specified rule.
    /// </summary>
    public static void AssertValid(this Grammar grammar, string ruleName, string input)
    {
        var isValid = grammar.TryValidate(input, ruleName, out var errorPos, out var errorMsg);
        Xunit.Assert.True(isValid, $"Expected '{input}' to be valid for rule '{ruleName}', but failed at position {errorPos}: {errorMsg}");
    }

    /// <summary>
    /// Asserts that the input is invalid according to the specified rule.
    /// </summary>
    /// <param name="expectedPosition">If specified, also asserts the error occurs at this position.</param>
    public static void AssertInvalid(this Grammar grammar, string ruleName, string input, int? expectedPosition = null)
    {
        var isValid = grammar.TryValidate(input, ruleName, out var errorPos, out var errorMsg);
        Xunit.Assert.False(isValid, $"Expected '{input}' to be invalid for rule '{ruleName}', but validation succeeded");
        
        if (expectedPosition.HasValue)
        {
            Xunit.Assert.Equal(expectedPosition.Value, errorPos);
        }
    }
}
