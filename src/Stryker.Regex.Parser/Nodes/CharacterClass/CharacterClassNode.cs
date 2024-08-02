using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stryker.Regex.Parser.Nodes.CharacterClass;

/// <summary>
/// RegexNode representing the characters used in a character class "[...]".
/// A CharacterClassNode should have at least one child.
/// The first child represents the characters contained in the character class and should be a CharacterClassCharacterSetNode.
/// A CharacterClassNode should have at most two children.
/// The second child represents a subtraction "[...-[...]]" and should be a CharacterClass.
/// </summary>
public class CharacterClassNode : RegexNode
{
    private const int _negatedChildSpanOffset = 2;
    private const int _unnegatedChildSpanOffset = 1;

    protected override int ChildSpanOffset => Negated ? _negatedChildSpanOffset : _unnegatedChildSpanOffset;
    public bool Negated { get; }
    public CharacterClassCharacterSetNode CharacterSet => ChildNodes.FirstOrDefault() as CharacterClassCharacterSetNode;
    public CharacterClassNode Subtraction => ChildNodes.ElementAtOrDefault(1) as CharacterClassNode;

    private CharacterClassNode(bool negated)
    {
        Negated = negated;
    }

    public CharacterClassNode(CharacterClassCharacterSetNode characterSet, bool negated)
        : base(characterSet)
    {
        Negated = negated;
    }

    public CharacterClassNode(CharacterClassCharacterSetNode characterSet, CharacterClassNode subtraction, bool negated)
        : base(new List<RegexNode> { characterSet, subtraction })
    {
        Negated = negated;
    }

    protected override RegexNode CopyInstance()
    {
        return new CharacterClassNode(Negated);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new($"{Prefix}[");

        if (Negated)
        {
            _ = stringBuilder.Append("^");
        }

        _ = stringBuilder.Append(CharacterSet);

        if (Subtraction != null)
        {
            _ = stringBuilder.Append($"-{Subtraction}");
        }

        _ = stringBuilder.Append("]");
        return stringBuilder.ToString();
    }
}
