using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using System.Collections.Generic;

namespace Stryker.Regex.Parser;

internal class GroupUnit
{
    internal RegexNode Node { get; set; }
    internal List<RegexNode> Alternates { get; } = [];
    internal List<RegexNode> Concatenation { get; } = [];

    internal GroupUnit(GroupNode node)
    {
        Node = node;
    }
}
