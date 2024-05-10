namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

/// <summary>
/// RegexNode representing an end of line token "$".
/// </summary>
public class EndOfLineNode : AnchorNode
{
    public override string ToString()
    {
        return $"{Prefix}$";
    }
}
