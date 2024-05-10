using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using System.Collections.Generic;

namespace RegexParser
{
    internal class GroupUnit
    {
        internal RegexNode Node { get; set; }
        internal List<RegexNode> Alternates { get; } = new List<RegexNode>();
        internal List<RegexNode> Concatenation { get; } = new List<RegexNode>();

        internal GroupUnit(GroupNode node)
        {
            Node = node; 
        }
    }
}