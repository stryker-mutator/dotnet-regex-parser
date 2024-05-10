namespace RegexParser.Nodes.AnchorNodes
{
    /// <summary>
    /// RegexNode representing a contiguous match token "\G".
    /// </summary>
    public class ContiguousMatchNode : AnchorNode
    {
        public override string ToString()
        {
            return $@"{Prefix}\G";
        }
    }
}
