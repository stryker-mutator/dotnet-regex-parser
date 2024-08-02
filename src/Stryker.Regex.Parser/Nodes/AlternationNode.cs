using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes
{
    /// <summary>
    /// RegexNode representing alternation "...|...|...".
    /// The children of the AlternationNode represent the alternates, seperated by pipes.
    /// An EmptyNode can be used to represent and alternate matching an empty string.
    /// </summary>
    public class AlternationNode : RegexNode
    {
        private AlternationNode()
        {
        }

        public AlternationNode(IEnumerable<RegexNode> childNodes) : base(childNodes) { }

        public override string ToString()
        {
            return string.Join("|", ChildNodes);
        }
    }
}
