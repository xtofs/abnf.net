namespace Abnf;

/// <summary>
/// Represents a grammar rule with its name and pattern for matching input strings.
/// This is an abstract representation independent of ABNF syntax.
/// </summary>
public sealed class GrammarRule
{
    public string Name { get; }
    public Pattern Pattern { get; }

    public GrammarRule(string name, Pattern pattern)
    {
        Name = name;
        Pattern = pattern;
    }
}

/// <summary>
/// Abstract pattern that can match against input strings.
/// </summary>
public abstract class Pattern
{
    /// <summary>
    /// Attempts to match this pattern against the input starting at the given position.
    /// Returns a MatchResult indicating success or failure with position information.
    /// </summary>
    public abstract MatchResult Match(string input, int position, GrammarContext context);

    /// <summary>
    /// Formats a character value for display in error messages.
    /// Printable characters (>= 32) are shown as characters, others as hex codes.
    /// </summary>
    protected static string FormatCharValue(int value)
    {
        if (value >= 32 && value <= 126)
        {
            return $"'{(char)value}' ({value:X})";
        }
        return $"{value:X}";
    }

    /// <summary>
    /// Formats a character range for display in error messages.
    /// Printable ranges are shown as characters, others as hex codes.
    /// </summary>
    protected static string FormatCharRange(int min, int max)
    {
        if (min >= 32 && max <= 126)
        {
            return $"'{(char)min}'-'{(char)max}' ({min:X}-{max:X})";
        }
        return $"{min:X}-{max:X}";
    }

    private Pattern() { }

    /// <summary>
    /// Terminal pattern - matches a literal string (case-sensitive or case-insensitive).
    /// </summary>
    public sealed class Terminal : Pattern
    {
        public string Value { get; }
        public bool IsCaseSensitive { get; }

        public Terminal(string value, bool isCaseSensitive = false)
        {
            Value = value;
            IsCaseSensitive = isCaseSensitive;
        }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            if (position + Value.Length > input.Length)
            {
                return MatchResult.Failure(position, $"Expected '{Value}' but reached end of input");
            }

            var substring = input.Substring(position, Value.Length);
            var comparison = IsCaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;

            if (string.Equals(substring, Value, comparison))
            {
                return MatchResult.Success(position + Value.Length);
            }

            return MatchResult.Failure(position, $"Expected '{Value}' but found '{substring}'");
        }
    }

    /// <summary>
    /// Character range pattern - matches a single character in a range (e.g., %x41-5A for A-Z).
    /// </summary>
    public sealed class CharacterRange : Pattern
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public CharacterRange(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            if (position >= input.Length)
            {
                return MatchResult.Failure(position, $"Expected character in range [{FormatCharRange(MinValue, MaxValue)}] but reached end of input");
            }

            var charValue = (int)input[position];
            if (charValue >= MinValue && charValue <= MaxValue)
            {
                return MatchResult.Success(position + 1);
            }

            return MatchResult.Failure(position, $"Expected character in range [{FormatCharRange(MinValue, MaxValue)}] but found '{input[position]}' ({charValue:X})");
        }
    }

    /// <summary>
    /// Specific character value pattern - matches a specific character by its numeric value.
    /// </summary>
    public sealed class CharacterValue : Pattern
    {
        public int Value { get; }

        public CharacterValue(int value)
        {
            Value = value;
        }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            if (position >= input.Length)
            {
                return MatchResult.Failure(position, $"Expected character {FormatCharValue(Value)} but reached end of input");
            }

            var charValue = (int)input[position];
            if (charValue == Value)
            {
                return MatchResult.Success(position + 1);
            }

            return MatchResult.Failure(position, $"Expected character {FormatCharValue(Value)} but found '{input[position]}' ({charValue:X})");
        }
    }

    /// <summary>
    /// Reference to another rule in the grammar.
    /// </summary>
    public sealed class RuleReference : Pattern
    {
        public string RuleName { get; }

        public RuleReference(string ruleName)
        {
            RuleName = ruleName;
        }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            if (!context.TryGetRule(RuleName, out var rule))
            {
                return MatchResult.Failure(position, $"Undefined rule '{RuleName}'");
            }

            // Check for left recursion
            if (context.IsInRecursionChain(RuleName, position))
            {
                return MatchResult.Failure(position, $"Left recursion detected in rule '{RuleName}'");
            }

            context.PushRule(RuleName, position);
            try
            {
                return rule.Pattern.Match(input, position, context);
            }
            finally
            {
                context.PopRule();
            }
        }
    }

    /// <summary>
    /// Sequence pattern - matches a sequence of patterns in order (concatenation).
    /// </summary>
    public sealed class Sequence : Pattern
    {
        public IReadOnlyList<Pattern> Elements { get; }

        public Sequence(IReadOnlyList<Pattern> elements)
        {
            Elements = elements;
        }

        public Sequence(params Pattern[] elements) : this((IReadOnlyList<Pattern>)elements) { }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            var currentPosition = position;

            foreach (var element in Elements)
            {
                var result = element.Match(input, currentPosition, context);
                if (!result.IsSuccess)
                {
                    return result;
                }
                currentPosition = result.Position;
            }

            return MatchResult.Success(currentPosition);
        }
    }

    /// <summary>
    /// Alternation pattern - tries multiple alternative patterns, succeeds with the first match.
    /// </summary>
    public sealed class Alternation : Pattern
    {
        public IReadOnlyList<Pattern> Alternatives { get; }

        public Alternation(IReadOnlyList<Pattern> alternatives)
        {
            Alternatives = alternatives;
        }

        public Alternation(params Pattern[] alternatives) : this((IReadOnlyList<Pattern>)alternatives) { }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            MatchResult? furthestFailure = null;

            foreach (var alternative in Alternatives)
            {
                var result = alternative.Match(input, position, context);
                if (result.IsSuccess)
                {
                    return result;
                }
                
                // Track the alternative that got furthest
                if (furthestFailure == null || result.FurthestPosition > furthestFailure.FurthestPosition)
                {
                    furthestFailure = result;
                }
            }

            // Report the error from the alternative that got furthest (most promising)
            return furthestFailure ?? MatchResult.Failure(position, "No alternatives provided");
        }
    }

    /// <summary>
    /// Repetition pattern - matches a pattern a specified number of times (min to max).
    /// </summary>
    public sealed class Repetition : Pattern
    {
        public Pattern Element { get; }
        public int? Min { get; }
        public int? Max { get; }

        public Repetition(Pattern element, int? min = null, int? max = null)
        {
            Element = element;
            Min = min;
            Max = max;
        }

        public override MatchResult Match(string input, int position, GrammarContext context)
        {
            var currentPosition = position;
            var count = 0;
            var minCount = Min ?? 0;
            var maxCount = Max ?? int.MaxValue;
            MatchResult? lastFailure = null;

            while (count < maxCount)
            {
                var result = Element.Match(input, currentPosition, context);
                if (!result.IsSuccess)
                {
                    lastFailure = result;
                    break;
                }
                currentPosition = result.Position;
                count++;

                // Prevent infinite loops on zero-width matches
                if (result.Position == position && count > 0)
                {
                    break;
                }
            }

            if (count < minCount)
            {
                // If we have a failure, use its message (more specific than "expected N occurrences")
                if (lastFailure != null)
                {
                    return MatchResult.FailureWithFurthest(
                        position, 
                        lastFailure.FurthestErrorMessage, 
                        lastFailure.FurthestPosition, 
                        lastFailure.FurthestErrorMessage);
                }
                
                return MatchResult.Failure(position, $"Expected at least {minCount} occurrences but found {count}");
            }

            return MatchResult.Success(currentPosition);
        }
    }
}
