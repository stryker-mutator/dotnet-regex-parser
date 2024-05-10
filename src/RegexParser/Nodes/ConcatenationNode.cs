using System.Collections.Generic;

namespace RegexParser.Nodes
{
    /// <summary>
    /// RegexNode used to concatenate multiple RegexNodes.
    /// </summary>
    public class ConcatenationNode : RegexNode
    {
        public ConcatenationNode() { }

        public ConcatenationNode(RegexNode childNode)
            : base(childNode)
        {
        }

        public ConcatenationNode(IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        { 
        }

        public override string ToString()
        {
            return string.Concat(ChildNodes);
        }
    }
}