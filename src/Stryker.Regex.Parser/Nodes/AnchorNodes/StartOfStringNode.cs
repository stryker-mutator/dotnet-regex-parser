namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

/// <summary>
/// RegexNode representing an start-of-string token "\A".
/// </summary>
public class StartOfStringNode : AnchorNode
{
    public override string ToString()
    {
        return $@"{Prefix}\A";
    }
}
