using Abnf.Parsing;

namespace Abnf;

/// <summary>
/// Provides convenience methods for working with ABNF grammars.
/// </summary>
public static class Abnf
{
    /// <summary>
    /// Parses an ABNF grammar string and converts it to a Grammar object.
    /// This is a convenience method that combines scanning, parsing, and conversion steps.
    /// </summary>
    /// <param name="input">The ABNF grammar text to parse.</param>
    /// <returns>A Grammar object that can be used for validation.</returns>
    /// <exception cref="SyntaxException">Thrown if the ABNF grammar is invalid.</exception>
    public static Grammar Parse(string input)
    {
        var tokens = Scanner.Scan(input);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        return AstToGrammarConverter.ToGrammar(ast);
    }
}
