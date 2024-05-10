using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing a non capture group "(?:...)".
/// </summary>
public class NonCaptureGroupNode : GroupNode
{
    private const int _childSpanOffset = 3;

    protected override int ChildSpanOffset => _childSpanOffset;

    public NonCaptureGroupNode()
    {
    }

    public NonCaptureGroupNode(RegexNode childNode)
        : base(childNode)
    {
    }

    public NonCaptureGroupNode(IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
    }

    public override string ToString()
    {
        return $"{Prefix}(?:{string.Concat(ChildNodes)})";
    }
}
