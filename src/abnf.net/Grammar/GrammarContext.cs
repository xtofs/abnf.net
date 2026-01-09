namespace Abnf;

/// <summary>
/// Context for grammar matching that tracks rule definitions and prevents infinite recursion.
/// </summary>
public sealed class GrammarContext
{
    private readonly Dictionary<string, GrammarRule> _rules;
    private readonly Stack<(string RuleName, int Position)> _recursionStack;

    public GrammarContext(IEnumerable<GrammarRule> rules)
    {
        _rules = rules.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
        _recursionStack = new Stack<(string, int)>();
    }

    public bool TryGetRule(string name, out GrammarRule rule)
    {
        return _rules.TryGetValue(name, out rule!);
    }

    public bool IsInRecursionChain(string ruleName, int position)
    {
        // Detect left recursion: same rule at the same position
        return _recursionStack.Any(entry => 
            entry.RuleName.Equals(ruleName, StringComparison.OrdinalIgnoreCase) && 
            entry.Position == position);
    }

    public void PushRule(string ruleName, int position)
    {
        _recursionStack.Push((ruleName, position));
    }

    public void PopRule()
    {
        if (_recursionStack.Count > 0)
        {
            _recursionStack.Pop();
        }
    }
}
