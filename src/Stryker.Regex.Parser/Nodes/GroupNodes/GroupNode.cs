using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

public abstract class GroupNode : RegexNode
{
    protected GroupNode()
    {
    }

    protected GroupNode(RegexNode childNode)
        : base(childNode)
    {
    }

    protected GroupNode(IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
    }
}
