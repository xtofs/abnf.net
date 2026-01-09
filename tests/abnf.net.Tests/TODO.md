
# TODO

## Completed Features

### ✅ Improve validation error reporting

**What was fixed:**

1. **Error position now reports actual failure point**
   - Previously: Reports the position where a rule *started* matching
   - Now: Reports the furthest position reached during parsing
   - Positions are 1-based for user-friendly messages (internally 0-based)
   - Example: `"(1+2"` now reports "Error at position 5" (not position 1)

2. **Cleaner error messages**
   - Previously: "No alternative matched. Tried: ... ; ... ; ..."
   - Now: Reports only the most promising alternative (one that got furthest)
   - Repetition errors now propagate the underlying cause instead of generic "Expected N occurrences"

**Example improvements achieved:**
```
Before: "(1+2" → Error at position 1: No alternative matched. Tried: Expected at least 1 occurrences but found 0; Expected ')' but reached end of input
After:  "(1+2" → Error at position 5: Expected ')' but reached end of input
```

### ✅ Improve README

Added comprehensive overview section to README.md covering:
- Library goals (BNF/ABNF support)
- ABNF parser with internal parse tree
- Abstract grammar representation (Alternation, Concatenation, Group, Repetition, Option, Literal, Rule)
- Validation engine with error reporting
- Updated project list with all demo applications

### ✅ Test asserts

Extension methods `GrammarExtensions.AssertValid()` and `GrammarExtensions.AssertInvalid()` provide clean test assertions:

```csharp
// Instead of:
var isValid = grammar.TryValidate("2*3", "term", out var errorPos, out var errorMsg);
Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");

// Use:
grammar.AssertValid("term", "2*3");
```

**Available methods:**
- `grammar.AssertValid(ruleName, input)` - Asserts input is valid
- `grammar.AssertInvalid(ruleName, input, expectedPosition?)` - Asserts input is invalid, optionally at a specific position

### ✅ Whitespace in demo

Created a second demo project (`abnf.net.DemoWithWhitespace`) that shows the same arithmetic expression grammar but with explicit whitespace handling:

```abnf
expr = term *( BWS ("+" / "-") BWS term )
term = factor *( BWS ("*" / "/") BWS factor )
factor = number / "(" BWS expr BWS ")"
number = 1*DIGIT
DIGIT = %x30-39
BWS = *( SP / HTAB )  ; Bad WhiteSpace (optional)
SP = %x20
HTAB = %x09
```

This accepts both `"1+2"` and `"1 + 2"` while staying true to ABNF's explicit whitespace handling.

**Key insight:** Having two demos side-by-side demonstrates ABNF's explicit whitespace philosophy:
- `abnf.net.Demo` - Basic grammar without whitespace (validates `"1+2"` but rejects `"1 + 2"`)
- `abnf.net.DemoWithWhitespace` - Grammar with optional whitespace rules (validates both)

Run the demos:
- `dotnet run --project samples/abnf.net.Demo`
- `dotnet run --project samples/abnf.net.DemoWithWhitespace`

## Future Enhancements

### Context tracking for error messages (nice-to-have)
- Could add: "Unclosed parenthesis - expected ')' to close '(' at position 1"
- Would require tracking opening delimiters and matching pairs
- Current messages are clear enough for MVP

### EBNF support
- Add parser for Extended BNF notation
- Convert EBNF to common grammar representation

