namespace Bnf.Grammar;

/// <summary>
/// Result of attempting to match a pattern against input.
/// Contains success/failure status, position, and error message if failed.
/// </summary>
public sealed class MatchResult
{
    public bool IsSuccess { get; }
    public int Position { get; }
    public string ErrorMessage { get; }

    private MatchResult(bool isSuccess, int position, string errorMessage)
    {
        IsSuccess = isSuccess;
        Position = position;
        ErrorMessage = errorMessage;
    }

    public static MatchResult Success(int position) 
        => new MatchResult(true, position, string.Empty);

    public static MatchResult Failure(int position, string errorMessage) 
        => new MatchResult(false, position, errorMessage);
}
