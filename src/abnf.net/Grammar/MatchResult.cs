namespace Abnf;

/// <summary>
/// Result of attempting to match a pattern against input.
/// Contains success/failure status, position, and error message if failed.
/// Tracks the furthest position reached to provide better error reporting.
/// </summary>
public sealed class MatchResult
{
    public bool IsSuccess { get; }
    public int Position { get; }
    public string ErrorMessage { get; }
    
    /// <summary>
    /// The furthest position reached during matching (even in failed attempts).
    /// This helps identify where parsing actually failed, not where it started.
    /// </summary>
    public int FurthestPosition { get; }
    
    /// <summary>
    /// Error message from the furthest position reached.
    /// This is usually the most relevant error for the user.
    /// </summary>
    public string FurthestErrorMessage { get; }

    private MatchResult(bool isSuccess, int position, string errorMessage, int furthestPosition, string furthestErrorMessage)
    {
        IsSuccess = isSuccess;
        Position = position;
        ErrorMessage = errorMessage;
        FurthestPosition = furthestPosition;
        FurthestErrorMessage = furthestErrorMessage;
    }

    public static MatchResult Success(int position) 
        => new MatchResult(true, position, string.Empty, position, string.Empty);

    public static MatchResult Failure(int position, string errorMessage) 
        => new MatchResult(false, position, errorMessage, position, errorMessage);
    
    /// <summary>
    /// Creates a failure result that preserves furthest position information from a deeper parse attempt.
    /// </summary>
    public static MatchResult FailureWithFurthest(int position, string errorMessage, int furthestPosition, string furthestErrorMessage)
        => new MatchResult(false, position, errorMessage, furthestPosition, furthestErrorMessage);
}
