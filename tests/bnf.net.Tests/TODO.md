
# TODO

## Features

### Improve validation error reporting

**Problem:** Current error messages are confusing and don't help users understand what went wrong in their input.

**Issues identified:**

1. **Wrong error position reported**
   - Reports the position where a rule *started* matching, not where it *failed*
   - Example: `"(1+2"` reports "Error at position 0" but the actual problem is at position 4 (end of input)

2. **Technical parser messages instead of user-friendly descriptions**
   - "Expected at least 1 occurrences but found 0" - doesn't say what was expected (digits)
   - "No alternative matched. Tried: ..." - exposes internal backtracking logic
   - Users can't understand or act on these messages

3. **Missing context**
   - Doesn't connect related parts (e.g., "expected ')' to close '(' at position 0")
   - Doesn't show what part of the grammar rule failed

**Current behavior:**
```
"(1+2" → Error at position 0: No alternative matched. Tried: Expected at least 1 occurrences but found 0; Expected ')' but reached end of input
```

**Desired behavior:**
```
"(1+2" → Error at position 4: Expected ')' but reached end of input
```

Or even better:
```
"(1+2" → Error at position 4: Unclosed parenthesis - expected ')' to close '(' at position 0
```

**Implementation approach:**

1. Track the **furthest position reached** during parsing (not just where rules start)
2. Report errors at the **actual failure point** in the input
3. Generate **human-readable messages** that describe what's wrong
4. For alternations, report only the most promising alternative (furthest match) instead of listing all attempts
5. Add context when relevant (matching pairs, what was expected based on grammar structure)

**Files to modify:**
- `src/bnf.net/Grammar/Grammar.cs` - validation logic
- `src/bnf.net/Grammar/GrammarRule.cs` - pattern matching with better error tracking

### improve readme

Add to the README.md a short excerpt about the goals and workings of the library

- the library is indented to support to work with https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form
- at the moment with a focus on ABNF and potentially in the future EBNF
- it has a parser for ABNF (with it's internal parse tree)
- and an abstract representation of BNF that is a (recursive) expression consisting of Alternation, Concatenation, Group, Repetition, Option, Literal and Rule terms.
- and lastly the ability to validate that a string conforms with the syntax or shows the reason and position of the failure


### test asserts
We see a lot of this in the tests
```csharp
        var isValid = grammar.TryValidate("2*3", "term", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
```        
what mechanism does xunit provide to write something equivalent to
```
     Assert.IsValidInput(grammar, "term", "2*3")
```