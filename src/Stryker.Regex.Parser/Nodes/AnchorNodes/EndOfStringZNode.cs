namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

/// <summary>
/// RegexNode representing an end of string or before ending newline token "\Z".
/// </summary>
public class EndOfStringZNode : AnchorNode
{
    public override string ToString()
    {
        return $@"{Prefix}\Z";
    }
}
