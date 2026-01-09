using System.Text.RegularExpressions;

namespace Abnf.Parsing;

public sealed partial class Scanner
{
    [GeneratedRegex(@"(?<Whitespace>[ \t]+)|(?<Comment>;[^\r\n]*)|(?<CRLF>\r\n|\n|\r)|(?<RuleName>[A-Za-z][A-Za-z0-9-]*)|(?<Repeat>([0-9]+)?\*[0-9]*)|(?<Integer>[0-9]+)(?!\*)|(?<CharVal>""[^""]*"")|(?<CaseSensitiveCharVal>'[^']*')|(?<ProseVal><[^>]*>)|(?<ValueRange>%[bBdDxX][0-9A-Fa-f]+-[0-9A-Fa-f]+)|(?<NumVal>%[bBdDxX][0-9A-Fa-f]+(?:\.[0-9A-Fa-f]+)*)|(?<Equal>=)|(?<Slash>/)|(?<Star>\*)|(?<OpenParen>\()|(?<CloseParen>\))|(?<OpenBracket>\[)|(?<CloseBracket>\])|(?<OpenAngle><)|(?<CloseAngle>>)|(?<Percent>%)(?![bBdDxX])|(?<OtherSymbol>[-])", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex TokenRegex();

    public static IEnumerable<Token> Scan(string input)
    {
        int line = 1, col = 1, pos = 0;
        var regex = TokenRegex();
        var m = regex.Match(input, pos);
        while (m.Success)
        {
            var group = m.Groups.Cast<Group>().Skip(1).SingleOrDefault(g => g.Success);
            if (group != null)
            {
                var groupIndex = m.Groups.Cast<Group>().ToList().IndexOf(group);
                var groupName = regex.GetGroupNames()[groupIndex];
                var value = group.Value;
                var tokenKind = Enum.TryParse<TokenKind>(groupName, out var kind) ? kind : TokenKind.OtherSymbol;
                yield return new Token(tokenKind, value, line, col);
                // Inline position update logic
                for (var i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\n')
                    {
                        line++;
                        col = 1;
                    }
                    else
                    {
                        col++;
                    }
                }
                pos += value.Length;
            }
            m = m.NextMatch();
        }
        yield return new Token(TokenKind.EndOfInput, string.Empty, line, col);
    }
}
