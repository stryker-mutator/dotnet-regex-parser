namespace RegexParser.Nodes.AnchorNodes
{
    /// <summary>
    /// RegexNode representing an non word boundary token "\B".
    /// </summary>
    public class NonWordBoundaryNode : AnchorNode
    {
        public override string ToString()
        {
            return $@"{Prefix}\B";
        }
    }
}
