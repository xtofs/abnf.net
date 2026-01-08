using System.Collections.Generic;
using System.Linq;

namespace Bnf.Parsing;

/// <summary>
/// Wraps an IEnumerable&lt;Token&gt; to provide lookahead capabilities for parsing.
/// This is essential for ABNF parsing where we need to distinguish between:
/// - New rule definitions (lines starting with rulename =)
/// - Continuation lines (part of the current rule's definition)
/// </summary>
public sealed class TokenStream(IEnumerable<Token> tokens)
{
    private readonly Token[] _tokens = tokens.ToArray();
    private int _position = 0;

    /// <summary>
    /// Gets the current token without consuming it
    /// </summary>
    public Token Current => _position < _tokens.Length ? _tokens[_position] : CreateEndOfInputToken();

    /// <summary>
    /// Checks if we're at the end of the token stream
    /// </summary>
    public bool IsAtEnd => _position >= _tokens.Length;

    /// <summary>
    /// Gets the current position in the token stream
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Advances to the next token
    /// </summary>
    public void MoveNext()
    {
        if (_position < _tokens.Length)
            _position++;
    }

    /// <summary>
    /// Looks ahead at the token at the specified offset from current position
    /// </summary>
    /// <param name="offset">Offset from current position (0 = current, 1 = next, etc.)</param>
    /// <returns>The token at the specified offset, or EndOfInput if beyond the end</returns>
    public Token Peek(int offset = 0)
    {
        var targetPos = _position + offset;
        return targetPos < _tokens.Length ? _tokens[targetPos] : CreateEndOfInputToken();
    }

    /// <summary>
    /// Checks if the current token matches the specified kind
    /// </summary>
    public bool Match(TokenKind kind) => Current.Kind == kind;

    /// <summary>
    /// Expects the current token to be of the specified kind, consuming it if it matches
    /// </summary>
    /// <param name="kind">The expected token kind</param>
    /// <param name="fileName">Optional filename for error reporting</param>
    /// <returns>The consumed token</returns>
    /// <exception cref="SyntaxException">Thrown if the current token doesn't match</exception>
    public Token Expect(TokenKind kind, string? fileName = null)
    {
        if (Current.Kind != kind)
            throw new SyntaxException($"Expected {kind} but found {Current.Kind}", fileName, Current.Line, Current.Column);
        
        var token = Current;
        MoveNext();
        return token;
    }

    /// <summary>
    /// Skips whitespace and comments but NOT CRLF (which is significant for rule boundaries)
    /// </summary>
    public void SkipTrivia()
    {
        while (Match(TokenKind.Whitespace) || Match(TokenKind.Comment))
            MoveNext();
    }

    /// <summary>
    /// Checks if the current position represents the start of a new rule definition.
    /// A new rule starts with: [whitespace] rulename [whitespace] =
    /// </summary>
    /// <returns>True if this position starts a new rule definition</returns>
    public bool IsAtRuleStart()
    {
        var pos = _position;
        
        // Skip any leading whitespace/comments (but not CRLF)
        while (pos < _tokens.Length && 
               (_tokens[pos].Kind == TokenKind.Whitespace || _tokens[pos].Kind == TokenKind.Comment))
        {
            pos++;
        }
        
        // Must have a rule name
        if (pos >= _tokens.Length || _tokens[pos].Kind != TokenKind.RuleName)
            return false;
        
        pos++; // Skip the rule name
        
        // Skip whitespace after rule name
        while (pos < _tokens.Length && _tokens[pos].Kind == TokenKind.Whitespace)
            pos++;
        
        // Must be followed by = or =/ 
        return pos < _tokens.Length && _tokens[pos].Kind == TokenKind.Equal;
    }

    /// <summary>
    /// Checks if we're at a rule boundary (CRLF followed by potential new rule or end of input)
    /// </summary>
    /// <returns>True if we're at a rule boundary</returns>
    public bool IsAtRuleBoundary()
    {
        if (!Match(TokenKind.CRLF))
            return false;
        
        // Look ahead past the CRLF to see if a new rule starts
        var pos = _position + 1; // Skip the CRLF
        
        // If we're at the end, it's definitely a boundary
        if (pos >= _tokens.Length)
            return true;
        
        // Skip any whitespace/comments after CRLF
        while (pos < _tokens.Length && 
               (_tokens[pos].Kind == TokenKind.Whitespace || _tokens[pos].Kind == TokenKind.Comment))
        {
            pos++;
        }
        
        // If we hit another CRLF or end of input, it's a boundary
        if (pos >= _tokens.Length || _tokens[pos].Kind == TokenKind.CRLF)
            return true;
        
        // If the next significant token is a rule name followed by =, it's a boundary
        if (pos < _tokens.Length && _tokens[pos].Kind == TokenKind.RuleName)
        {
            var nextPos = pos + 1;
            while (nextPos < _tokens.Length && _tokens[nextPos].Kind == TokenKind.Whitespace)
                nextPos++;
            
            return nextPos < _tokens.Length && _tokens[nextPos].Kind == TokenKind.Equal;
        }
        
        // Otherwise, it's a continuation line
        return false;
    }

    /// <summary>
    /// Advances past any CRLF tokens, but only if they don't represent rule boundaries
    /// </summary>
    public void SkipContinuationLines()
    {
        while (Match(TokenKind.CRLF) && !IsAtRuleBoundary())
        {
            MoveNext();
            SkipTrivia(); // Skip whitespace after CRLF on continuation lines
        }
    }

    private Token CreateEndOfInputToken()
    {
        var lastToken = _tokens.Length > 0 ? _tokens[_tokens.Length - 1] : null;
        return new Token(TokenKind.EndOfInput, string.Empty, lastToken?.Line ?? 0, lastToken?.Column ?? 0);
    }
}
