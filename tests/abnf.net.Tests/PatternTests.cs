namespace Abnf.Tests;

public class PatternTests
{
    [Fact]
    public void Repetition_ZeroMatches_Succeeds()
    {
        // Pattern: 0* DIGIT means zero or more digits
        var digit = new Pattern.CharacterRange(0x30, 0x39); // 0-9
        var repetition = new Pattern.Repetition(digit, min: 0, max: null);
        
        var context = new GrammarContext(Array.Empty<GrammarRule>());
        var result = repetition.Match("abc", 0, context);
        
        Assert.True(result.IsSuccess, $"Repetition with min=0 should succeed even with no matches. Error: {result.ErrorMessage}");
        Assert.Equal(0, result.Position); // Should not advance position
    }

    [Fact]
    public void Sequence_FirstElementFails_ReturnsFailure()
    {
        var literal = new Pattern.Terminal("*", false);
        var digit = new Pattern.CharacterRange(0x30, 0x39);
        var sequence = new Pattern.Sequence(literal, digit);
        
        var context = new GrammarContext(Array.Empty<GrammarRule>());
        var result = sequence.Match("123", 0, context);
        
        Assert.False(result.IsSuccess, "Sequence should fail when first element doesn't match");
        Assert.Equal(0, result.Position);
    }

    [Fact]
    public void Repetition_OfSequence_ZeroMatches_Succeeds()
    {
        // Pattern: *( "*" DIGIT ) means zero or more occurrences of (* followed by digit)
        var literal = new Pattern.Terminal("*", false);
        var digit = new Pattern.CharacterRange(0x30, 0x39);
        var sequence = new Pattern.Sequence(literal, digit);
        var repetition = new Pattern.Repetition(sequence, min: 0, max: null);
        
        var context = new GrammarContext(Array.Empty<GrammarRule>());
        var result = repetition.Match("123", 0, context);
        
        Assert.True(result.IsSuccess, $"Repetition of sequence with min=0 should succeed with zero matches. Error: {result.ErrorMessage}");
        Assert.Equal(0, result.Position); // Repetition tried to match but failed on first attempt, should succeed with 0 matches
    }
}
