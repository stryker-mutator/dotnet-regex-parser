using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing a lookaround group "(?=...)", "(?!...)", "(?&lt;=)", "(?&lt;!)"
/// </summary>
public class LookaroundGroupNode : GroupNode
{
    private const int _lookaheadChildSpanOffset = 4;
    private const int _lookbehindChildSpanOffset = 3;

    protected override int ChildSpanOffset => Lookahead ? _lookaheadChildSpanOffset : _lookbehindChildSpanOffset;
    public bool Lookahead { get; }
    public bool Possitive { get; }

    public LookaroundGroupNode(bool lookahead, bool possitive)
    {
        Lookahead = lookahead;
        Possitive = possitive;
    }

    public LookaroundGroupNode(bool lookahead, bool negative, RegexNode childNode)
        : base(childNode)
    {
        Lookahead = lookahead;
        Possitive = negative;
    }

    public LookaroundGroupNode(bool lookahead, bool negative, IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
        Lookahead = lookahead;
        Possitive = negative;
    }

    protected override RegexNode CopyInstance()
    {
        return new LookaroundGroupNode(Lookahead, Possitive);
    }

    public override string ToString()
    {
        return $"{Prefix}(?{(Lookahead ? "" : "<")}{(Possitive ? "=" : "!")}{string.Concat(ChildNodes)})";
    }
}
