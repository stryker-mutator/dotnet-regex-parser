using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.CharacterClass;

/// <summary>
/// RegexNode representing the characters used in a character class "[...]".
/// </summary>
public class CharacterClassCharacterSetNode : RegexNode
{
    public CharacterClassCharacterSetNode() { }

    public CharacterClassCharacterSetNode(RegexNode childNode)
        : base(childNode)
    {
    }

    public CharacterClassCharacterSetNode(IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
    }

    public override string ToString()
    {
        return string.Concat(ChildNodes);
    }
}
