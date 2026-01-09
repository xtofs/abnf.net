using System.Collections.Generic;
using System.Linq;
using Bnf.Ast;

namespace Bnf.Parsing;

/// <summary>
/// Clean implementation of ABNF parser based on the architecture documented in ARCHITECTURE.md
/// This parser operates on tokens produced by the Scanner and builds an AST following the ABNF grammar.
/// Key principle: ABNF rules can span multiple lines, so we must distinguish between:
/// - Rule boundaries: lines starting with rulename =
/// - Continuation lines: all other lines that are part of the current rule
/// </summary>
public sealed class Parser(IEnumerable<Token> tokens, string? fileName = null)
{
    private readonly TokenStream _tokens = new TokenStream(tokens);
    private readonly string? _fileName = fileName;

    private bool Match(TokenKind kind) => _tokens.Match(kind);

    private Token Expect(TokenKind kind)
    {
        return _tokens.Expect(kind, _fileName);
    }

    /// <summary>
    /// Skips whitespace and comments but NOT CRLF (which is significant for rule boundaries)
    /// </summary>
    private void SkipTrivia()
    {
        _tokens.SkipTrivia();
    }

    /// <summary>
    /// Parses: rulelist = 1*( rule / (*c-wsp c-nl) )
    /// In our token implementation: sequence of rules separated by CRLF, with optional comments/whitespace
    /// </summary>
    public AstNode.RuleList ParseRuleList()
    {
        var rules = new List<AstNode.Rule>();
        
        // Skip any initial trivia
        while (Match(TokenKind.Whitespace) || Match(TokenKind.Comment) || Match(TokenKind.CRLF))
            _tokens.MoveNext();
        
        while (!Match(TokenKind.EndOfInput))
        {
            if (_tokens.IsAtRuleStart())
            {
                rules.Add(ParseRule());
            }
            else
            {
                // Skip trivia between rules
                if (Match(TokenKind.Whitespace) || Match(TokenKind.Comment) || Match(TokenKind.CRLF))
                {
                    _tokens.MoveNext();
                }
                else
                {
                    var current = _tokens.Current;
                    throw new SyntaxException($"Expected rule name but found {current.Kind}", _fileName, current.Line, current.Column);
                }
            }
        }
        
        return new AstNode.RuleList(rules);
    }

    /// <summary>
    /// Parses: rule = rulename defined-as elements c-nl
    /// In our token implementation: RuleName Equal Expression (potentially spanning multiple lines)
    /// Rules can span multiple lines - only a new rulename = starts a new rule
    /// </summary>
    private AstNode.Rule ParseRule()
    {
        var name = Expect(TokenKind.RuleName).Value;
        SkipTrivia();
        Expect(TokenKind.Equal);
        SkipTrivia();
        
        var expr = ParseAlternation();
        
        // Rule ends when we hit a rule boundary (CRLF followed by new rule or end of input)
        SkipTrivia(); // Allow trailing whitespace/comments
        _tokens.SkipContinuationLines(); // Skip CRLF that are part of this rule
        
        if (Match(TokenKind.CRLF) && _tokens.IsAtRuleBoundary())
            _tokens.MoveNext();
        else if (!Match(TokenKind.EndOfInput))
        {
            var current = _tokens.Current;
            throw new SyntaxException("Expected end of rule (CRLF)", _fileName, current.Line, current.Column);
        }
        
        return new AstNode.Rule(name, expr);
    }

    /// <summary>
    /// Parses: alternation = concatenation *("/" concatenation)
    /// Can now span multiple lines - only rule boundaries (new rulename =) stop alternation
    /// </summary>
    private AstNode.Expression ParseAlternation()
    {
        var options = new List<AstNode.Expression> { ParseConcatenation() };
        
        while (true)
        {
            SkipTrivia();
            _tokens.SkipContinuationLines(); // Allow alternation to continue across lines
            
            // Stop at rule boundary or end of input
            if (_tokens.IsAtRuleBoundary() || Match(TokenKind.EndOfInput))
                break;
                
            if (Match(TokenKind.Slash))
            {
                _tokens.MoveNext();
                SkipTrivia();
                _tokens.SkipContinuationLines(); // Allow continuation after slash
                options.Add(ParseConcatenation());
            }
            else
            {
                break;
            }
        }
        
        return options.Count == 1 ? options[0] : new AstNode.Expression.Alternation(options);
    }

    /// <summary>
    /// Parses: concatenation = repetition *(1*c-wsp repetition)
    /// Can now span multiple lines - only rule boundaries stop concatenation
    /// </summary>
    private AstNode.Expression ParseConcatenation()
    {
        var elements = new List<AstNode.Expression> { ParseRepetition() };
        
        while (true)
        {
            SkipTrivia();
            _tokens.SkipContinuationLines(); // Allow concatenation to continue across lines
            
            // Stop at rule boundary or end of input
            if (_tokens.IsAtRuleBoundary() || Match(TokenKind.EndOfInput))
                break;
                
            // Check if next token can start an element
            if (IsElementStart())
            {
                elements.Add(ParseRepetition());
            }
            else
            {
                break;
            }
        }
        
        return elements.Count == 1 ? elements[0] : new AstNode.Expression.Concatenation(elements);
    }

    /// <summary>
    /// Parses: repetition = [repeat] element
    /// </summary>
    private AstNode.Expression ParseRepetition()
    {
        int? min = null, max = null;
        bool hasRepetition = false;
        
        if (Match(TokenKind.Repeat))
        {
            var repeatToken = Expect(TokenKind.Repeat);
            (min, max) = repeatToken.GetRepetitionBounds();
            hasRepetition = true;
        }
        
        // Skip whitespace after repetition operator
        if (hasRepetition)
        {
            SkipTrivia();
        }
        
        var element = ParseElement();
        
        // If we consumed a repetition operator, wrap the element in a Repetition node
        if (hasRepetition)
        {
            return new AstNode.Expression.Repetition(min, max, element);
        }
        
        return element;
    }

    private AstNode.Expression ParseElement()
    {
        if (Match(TokenKind.RuleName))
        {
            var name = Expect(TokenKind.RuleName).Value;
            return new AstNode.Expression.RuleRef(name);
        }
        
        if (Match(TokenKind.OpenParen))
        {
            return ParseGroup();
        }
        
        if (Match(TokenKind.OpenBracket))
        {
            return ParseOption();
        }
        
        if (Match(TokenKind.CharVal))
        {
            var token = Expect(TokenKind.CharVal);
            return new AstNode.Expression.Literal(token.GetStringValue(), isCaseSensitive: false);
        }

        if (Match(TokenKind.CaseSensitiveCharVal))
        {
            var token = Expect(TokenKind.CaseSensitiveCharVal);
            return new AstNode.Expression.Literal(token.GetStringValue(), isCaseSensitive: true);
        }

        if (Match(TokenKind.NumVal))
        {
            var value = Expect(TokenKind.NumVal).Value;
            return new AstNode.Expression.NumberVal(value);
        }
        
        if (Match(TokenKind.ValueRange))
        {
            var value = Expect(TokenKind.ValueRange).Value;
            return new AstNode.Expression.NumberVal(value);
        }
        
        if (Match(TokenKind.ProseVal))
        {
            var value = Expect(TokenKind.ProseVal).Value;
            return new AstNode.Expression.ProseVal(value);
        }
        
        if (Match(TokenKind.Integer))
        {
            var value = Expect(TokenKind.Integer).Value;
            return new AstNode.Expression.NumberVal(value);
        }
        
        throw new SyntaxException($"Expected element but found {_tokens.Current.Kind}", _fileName, _tokens.Current.Line, _tokens.Current.Column);
    }

    /// <summary>
    /// Parses: group = "(" *c-wsp alternation *c-wsp ")"
    /// </summary>
    private AstNode.Expression ParseGroup()
    {
        Expect(TokenKind.OpenParen);
        SkipTrivia();
        var inner = ParseAlternation();
        SkipTrivia();
        Expect(TokenKind.CloseParen);
        return new AstNode.Expression.Group(inner);
    }

    /// <summary>
    /// Parses: option = "[" *c-wsp alternation *c-wsp "]"
    /// </summary>
    private AstNode.Expression ParseOption()
    {
        Expect(TokenKind.OpenBracket);
        SkipTrivia();
        var inner = ParseAlternation();
        SkipTrivia();
        Expect(TokenKind.CloseBracket);
        return new AstNode.Expression.Option(inner);
    }

    /// <summary>
    /// Checks if the current token can start an element
    /// </summary>
    private bool IsElementStart()
    {
        return Match(TokenKind.RuleName) || 
               Match(TokenKind.OpenParen) || 
               Match(TokenKind.OpenBracket) || 
               Match(TokenKind.CharVal) || 
               Match(TokenKind.CaseSensitiveCharVal) ||
               Match(TokenKind.NumVal) || 
               Match(TokenKind.ValueRange) || 
               Match(TokenKind.ProseVal) || 
               Match(TokenKind.Integer) ||
               Match(TokenKind.Repeat); // Repetition operator (including standalone *)
    }
}

public sealed class SyntaxException(string message, string? file = null, int line = 0, int column = 0) : Exception(FormatMessage(message, file, line, column))
{
    private static string FormatMessage(string message, string? file, int line, int column)
    {
        var filePart = string.IsNullOrEmpty(file) ? "<input>" : file;
        return $"{filePart}({line},{column}): {message}";
    }
}
