using System.Collections.Generic;
using System.Linq;

namespace Stryker.Regex.Parser.Nodes.CharacterClass
{
    /// <summary>
    /// RegexNode representing a character range used in a character class "x-y" where y >= x.
    /// A CharacterClassCharacterSetNode should have exactly two children.
    /// The first child represents the start of the range.
    /// The second child represents the end of the range.
    /// The children should be CharacterNodes or EscapeCharacterNodes.
    /// </summary>
    public class CharacterClassRangeNode : RegexNode
    {
        public RegexNode Start => ChildNodes.FirstOrDefault();
        public RegexNode End => ChildNodes.LastOrDefault();

        private CharacterClassRangeNode()
        {
        }

        public CharacterClassRangeNode(RegexNode start, RegexNode end)
            : base(new List<RegexNode> { start, end })
        {
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }
    }
}
