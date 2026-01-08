
# TODO

## Features

### improve readme

Add to the README.md a short excerpt about the goals and workings of the library

- the library is indented to support to work with https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form
- at the moment with a focus on ABNF and potentially in the future EBNF
- it has a parser for ABNF (with it's internal parse tree)
- and an abstract representation of BNF that is a (recursive) expression consisting of Alternation, Concatenation, Group, Repetition, Option, Literal and Rule terms.
- and lastly the ability to validate that a string conforms with the syntax or shows the reason and position of the failure


### cenvenience for standard use case
let's add a convenience function "Parse" on a static "Abnf" class that does the steps in the CreateGrammar method line 83

### test asserts
We see a lot of this in the tests
```csharp
        var isValid = grammar.TryValidate("2*3", "term", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
```        
what mechanism does xunit provide to write something equivalent to
```
     Assert.Valid("2*3", "term")
```