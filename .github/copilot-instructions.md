# ABNF Parser & BNF.NET Development Instructions

## Token Design Principles

### Token Value Decoding
- **Token class should encapsulate all token value parsing logic**
- Never parse token values directly in the Parser - always use Token methods
- Use `Token.GetStringValue()` for CharVal, CaseSensitiveCharVal, NumVal tokens
- Use `Token.GetRepetitionBounds()` for Repeat tokens
- Parser should work with decoded values, not raw token strings

### Scanner Token Types
- The Scanner regex uses alternation with order precedence
- The `Repeat` pattern `([0-9]+)?\*[0-9]*` matches ALL repetition forms:
  - `*` (standalone star)
  - `1*` (min only)
  - `*5` (max only)
  - `1*3` (min and max)
- There is no separate `Star` token - it's redundant since `Repeat` matches `*`
- When adding tokens, verify Scanner regex order to avoid dead patterns

## Parser Architecture

### Separation of Concerns
- **ParseRepetition()** handles the repetition operator and wraps elements
- **ParseElement()** handles atomic elements (literals, rules, groups, etc.)
- This separation follows the ABNF grammar: `repetition = [repeat] element`
- Don't conflate repetition parsing with element parsing in the same method

### Repetition Parsing Pattern
```csharp
private AstNode.Expression ParseRepetition()
{
    bool hasRepetition = false;
    int? min = null, max = null;
    
    if (Match(TokenKind.Repeat))
    {
        var token = Expect(TokenKind.Repeat);
        (min, max) = token.GetRepetitionBounds();
        hasRepetition = true;
    }
    
    if (hasRepetition)
        SkipTrivia(); // Allow whitespace after operator
    
    var element = ParseElement();
    
    return hasRepetition 
        ? new AstNode.Expression.Repetition(min, max, element)
        : element;
}
```

### Element Parsing Pattern
- ParseElement() should handle atomic grammar elements
- Use token decoding methods, not manual string manipulation:
  ```csharp
  // CORRECT:
  var token = Expect(TokenKind.CharVal);
  return new AstNode.Expression.Literal(token.GetStringValue(), isCaseSensitive: false);
  
  // INCORRECT:
  var value = Expect(TokenKind.CharVal).Value;
  value = value.Trim('"');
  return new AstNode.Expression.Literal(value, isCaseSensitive: false);
  ```

## ABNF Semantics

### Repetition Bounds
- `*` means 0 or more: `(min: null, max: null)`
- `1*` means 1 or more: `(min: 1, max: null)`
- `*5` means 0 to 5: `(min: null, max: 5)`
- `1*3` means 1 to 3: `(min: 1, max: 3)`
- Null values are intentional - they represent "unbounded" in the specification

### Token Value Representation
- Keep raw token values in the scanner (don't pre-process)
- Decode values on-demand through Token methods
- This allows inspection of original input when needed

## Testing Strategy

### Parser Testing
- Test scanner tokenization separately from parser logic
- Verify that token patterns match as expected (check regex order)
- Test repetition parsing with and without whitespace
- Include edge cases: standalone `*`, spacing variations

### Grammar Validation Testing
- Separate pattern matching tests from conversion tests
- Test AST-to-Grammar conversion independently
- Validate end-to-end with realistic grammars (arithmetic expressions)
