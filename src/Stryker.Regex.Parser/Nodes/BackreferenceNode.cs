namespace Stryker.Regex.Parser.Nodes
{
    /// <summary>
    /// RegexNode representing a backreference "\1".
    /// </summary>
    public class BackreferenceNode : RegexNode
    {
        public int GroupNumber { get; }

        public BackreferenceNode(int groupNumber)
        {
            GroupNumber = groupNumber;
        }

        protected override RegexNode CopyInstance()
        {
            return new BackreferenceNode(GroupNumber);
        }

        public override string ToString()
        {
            return $@"{Prefix}\{GroupNumber}";
        }
    }
}
