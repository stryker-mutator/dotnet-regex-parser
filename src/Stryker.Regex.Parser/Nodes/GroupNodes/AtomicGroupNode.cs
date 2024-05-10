using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes
{
    /// <summary>
    /// RegexNode representing an atomic group "(?>...)".
    /// </summary>
    public class AtomicGroupNode : GroupNode
    {
        private const int _childSpanOffset = 3;

        protected override int ChildSpanOffset => _childSpanOffset;

        public AtomicGroupNode()
        {
        }

        public AtomicGroupNode(RegexNode childNode)
            : base(childNode)
        {
        }

        public AtomicGroupNode(IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        {
        }

        public override string ToString()
        {
            return $"{Prefix}(?>{string.Concat(ChildNodes)})";
        }
    }
}
