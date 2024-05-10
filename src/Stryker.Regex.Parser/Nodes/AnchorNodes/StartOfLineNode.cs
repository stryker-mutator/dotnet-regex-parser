namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

/// <summary>
/// RegexNode representing an start-of-line token "^".
/// </summary>
public class StartOfLineNode : AnchorNode
{
    public override string ToString()
    {
        return $"{Prefix}^";
    }
}
