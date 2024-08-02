using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing a capture group "(...)".
/// </summary>
public class CaptureGroupNode : GroupNode
{
    private const int _childSpanOffset = 1;

    protected override int ChildSpanOffset => _childSpanOffset;

    public CaptureGroupNode()
    {
    }

    public CaptureGroupNode(RegexNode childNode)
        : base(childNode)
    {
    }

    public CaptureGroupNode(IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
    }

    public override string ToString()
    {
        return $"{Prefix}({string.Concat(ChildNodes)})";
    }
}
