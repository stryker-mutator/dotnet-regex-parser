using RegexParser.Nodes;

namespace RegexParser
{
    /// <summary>
    /// RegexTree is a wrapper for a RegexNode tree.
    /// </summary>
    public class RegexTree
    {
        public RegexNode Root { get; }

        public RegexTree(RegexNode root)
        {
            Root = root;
        }
    }
}