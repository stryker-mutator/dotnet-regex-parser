namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

/// <summary>
/// RegexNode representing an end of string token "\z".
/// </summary>
public class EndOfStringNode : AnchorNode
{
    public override string ToString()
    {
        return $@"{Prefix}\z";
    }
}
