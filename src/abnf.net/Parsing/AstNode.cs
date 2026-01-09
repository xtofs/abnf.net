namespace Abnf.Parsing;

public abstract class AstNode
{
    private AstNode() { }

    public sealed class RuleList(IReadOnlyList<AstNode.Rule> rules) : AstNode
    {
        public IReadOnlyList<Rule> Rules { get; } = rules;
    }

    public sealed class Rule(string name, AstNode.Expression expr) : AstNode
    {
        public string Name { get; } = name;
        public Expression Expr { get; } = expr;
    }

    /// <summary>
    /// Represents an expression , the right hand side of a rule, in the ABNF AST.
    /// 
    public abstract class Expression : AstNode
    {
        private Expression() { }

        public sealed class Alternation(IReadOnlyList<AstNode.Expression> options) : Expression
        {
            public IReadOnlyList<Expression> Options { get; } = options;
        }

        public sealed class Concatenation(IReadOnlyList<AstNode.Expression> elements) : Expression
        {
            public IReadOnlyList<Expression> Elements { get; } = elements;
        }

        public sealed class Repetition(int? min, int? max, AstNode.Expression element) : Expression
        {
            public int? Min { get; } = min;
            public int? Max { get; } = max;
            public Expression Element { get; } = element;
        }

        public sealed class Group(AstNode.Expression inner) : Expression
        {
            public Expression Inner { get; } = inner;
        }

        public sealed class Option(AstNode.Expression inner) : Expression
        {
            public Expression Inner { get; } = inner;
        }

        public sealed class Literal(string value, bool isCaseSensitive = false) : Expression
        {
            public string Value { get; } = value;
            public bool IsCaseSensitive { get; } = isCaseSensitive;
        }

        public sealed class ProseVal(string value) : Expression
        {
            public string Value { get; } = value;
        }

        public sealed class NumberVal(string value) : Expression
        {
            public string Value { get; } = value;
        }

        public sealed class RuleRef(string name) : Expression
        {
            public string Name { get; } = name;
        }
    }
}
