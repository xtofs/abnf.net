
# TODO

## Features

### Improve validation error reporting

**✅ COMPLETED** - Basic improvements implemented

**What was fixed:**

1. ✅ **Error position now reports actual failure point**
   - Previously: Reports the position where a rule *started* matching
   - Now: Reports the furthest position reached during parsing
   - Positions are 1-based for user-friendly messages (internally 0-based)
   - Example: `"(1+2"` now reports "Error at position 5" (not position 1)

2. ✅ **Cleaner error messages**
   - Previously: "No alternative matched. Tried: ... ; ... ; ..."
   - Now: Reports only the most promising alternative (one that got furthest)
   - Repetition errors now propagate the underlying cause instead of generic "Expected N occurrences"

3. **Still TODO: Context tracking** (nice-to-have enhancement)
   - Could add: "Unclosed parenthesis - expected ')' to close '(' at position 1"
   - Would require tracking opening delimiters and matching pairs
   - Current messages are clear enough for MVP

**Example improvements achieved:**
```
Before: "(1+2" → Error at position 1: No alternative matched. Tried: Expected at least 1 occurrences but found 0; Expected ')' but reached end of input
After:  "(1+2" → Error at position 5: Expected ')' but reached end of input
```

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

### whitespace in demo
My understanding is that ABNF is a bit picky with whitespace and has to have this explicitly encoded in the rules
I don't want to change that behavior because I don't want to change ABNF but the grammar in the demo is then probably not very realistic
