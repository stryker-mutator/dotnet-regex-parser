namespace RegexParser.Nodes.AnchorNodes
{
    /// <summary>
    /// RegexNode representing an word boundary token "\b".
    /// </summary>
    public class WordBoundaryNode : AnchorNode
    {
        public override string ToString()
        {
            return $@"{Prefix}\b";
        }
    }
}
