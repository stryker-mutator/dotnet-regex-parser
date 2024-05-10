namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing a single character "a".
/// </summary>
public class CharacterNode : RegexNode
{
    public char Character { get; }

    public CharacterNode(char ch)
    {
        Character = ch;
    }

    public override string ToString()
    {
        return $"{Prefix}{Character}";
    }

    protected override RegexNode CopyInstance()
    {
        return new CharacterNode(Character);
    }
}
