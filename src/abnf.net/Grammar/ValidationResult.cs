namespace Abnf;

/// <summary>
/// Result of validating an input string against a grammar rule.
/// Provides structured error information and utilities for position formatting.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// True if validation succeeded, false otherwise.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// The 0-based position where the error occurred (if validation failed).
    /// Use GetLineColumn() to convert to line/column format.
    /// </summary>
    public int ErrorPosition { get; }
    
    /// <summary>
    /// The error message (if validation failed).
    /// </summary>
    public string ErrorMessage { get; }

    private ValidationResult(bool isSuccess, int errorPosition, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorPosition = errorPosition;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() 
        => new ValidationResult(true, 0, string.Empty);

    /// <summary>
    /// Creates a failed validation result with error position and message.
    /// </summary>
    /// <param name="errorPosition">0-based position where the error occurred</param>
    /// <param name="errorMessage">Description of the error</param>
    public static ValidationResult Failure(int errorPosition, string errorMessage)
        => new ValidationResult(false, errorPosition, errorMessage);

    /// <summary>
    /// Converts the error position to 1-based line and column numbers.
    /// Lines and columns are 1-based for user-friendly display.
    /// </summary>
    /// <param name="input">The input string that was validated</param>
    /// <returns>A tuple of (line, column), both 1-based</returns>
    public (int line, int column) GetLineColumn(string input)
    {
        if (IsSuccess || string.IsNullOrEmpty(input))
        {
            return (1, 1);
        }

        int line = 1;
        int column = 1;
        int position = 0;

        while (position < ErrorPosition && position < input.Length)
        {
            if (input[position] == '\n')
            {
                line++;
                column = 1;
            }
            else if (input[position] != '\r') // Don't count \r in column
            {
                column++;
            }
            position++;
        }

        return (line, column);
    }

    /// <summary>
    /// Formats the error for display with 1-based line and column numbers.
    /// </summary>
    /// <param name="input">The input string that was validated</param>
    /// <returns>A formatted error string, or empty string if validation succeeded</returns>
    public string FormatError(string input)
    {
        if (IsSuccess)
        {
            return string.Empty;
        }

        var (line, column) = GetLineColumn(input);
        return $"Line {line}, Column {column}: {ErrorMessage}";
    }

    /// <summary>
    /// Formats the error with 1-based position index.
    /// </summary>
    /// <returns>A formatted error string, or empty string if validation succeeded</returns>
    public string FormatErrorWithPosition()
    {
        if (IsSuccess)
        {
            return string.Empty;
        }

        return $"Position {ErrorPosition + 1}: {ErrorMessage}";
    }
}
