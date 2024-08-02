namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing an empty string "".
/// The EmptyNode can be used to represent an empty alternate in an AlternationNode.
/// An EmptyNode with a prefix can be used to represent a comment group at the end of a regex.
/// </summary>
public class EmptyNode : RegexNode
{
    public override string ToString()
    {
        return $"{Prefix}";
    }
}
