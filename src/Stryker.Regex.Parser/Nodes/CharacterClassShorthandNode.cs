namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing a character class shorthand "\d".
/// </summary>
public class CharacterClassShorthandNode : RegexNode
{
    public char Shorthand { get; }

    public CharacterClassShorthandNode(char shorthand)
    {
        Shorthand = shorthand;
    }

    protected override RegexNode CopyInstance()
    {
        return new CharacterClassShorthandNode(Shorthand);
    }

    public override string ToString()
    {
        return $@"{Prefix}\{Shorthand}";
    }
}
