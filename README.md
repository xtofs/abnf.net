# bnf.net

A .NET library for working with [Backus-Naur Form](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form) grammars.

## Overview

This library provides comprehensive support for working with BNF grammars, with a current focus on [ABNF (Augmented Backus-Naur Form)](https://datatracker.ietf.org/doc/html/rfc5234). Future versions may include support for EBNF.

**Key components:**

1. **ABNF Parser** - Parse ABNF grammar definitions with a complete internal parse tree
2. **Abstract Grammar Representation** - A recursive expression model consisting of:
   - Alternation (`/`) - choose between alternatives
   - Concatenation (space) - sequence of elements
   - Repetition (`*`, `n*m`) - repeat elements
   - Option (`[...]`) - optional elements
   - Group (`(...)`) - grouping
   - Literal - character values and ranges
   - Rule - named grammar rules
3. **Validation Engine** - Validate input strings against grammar rules with detailed error reporting (position and reason for failures)

See the demo projects for practical examples of using the library.

## Projects

- **abnf.net** - Core class library
- **abnf.net.Demo** - Arithmetic expression grammar demo (run with `ws` argument for whitespace support)
- **abnf.net.Tests** - Unit tests

## Running the Demo

```bash
# Basic mode (no whitespace around operators)
dotnet run --project samples/abnf.net.Demo

# With whitespace support
dotnet run --project samples/abnf.net.Demo -- ws
```

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## NuGet Package

The library is available as a NuGet package: [Abnf.Net](https://www.nuget.org/packages/Abnf.Net)

```bash
dotnet add package Abnf.Net --prerelease
```

For package publishing and release information, see [docs/NUGET_PUBLISHING.md](docs/NUGET_PUBLISHING.md).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
