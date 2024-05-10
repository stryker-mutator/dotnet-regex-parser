using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes
{
    /// <summary>
    /// RegexNode representing a balancing group "(?&lt;Name-BalancedGroupName&gt;...)".
    /// Name is the name of the balancing group and is optional.
    /// BalancedGroupName is the name of the group being balanced and is required.
    /// </summary>
    public class BalancingGroupNode : GroupNode
    {
        private const int _childSpanOffset = 5;

        protected override int ChildSpanOffset => BalancedGroupName.Length + (Name?.Length ?? 0) + _childSpanOffset;
        public string Name { get; }
        public string BalancedGroupName { get; }
        public bool UseQuotes { get; }

        public BalancingGroupNode(string balancedGroupName, bool useQuotes)
        {
            BalancedGroupName = balancedGroupName;
            UseQuotes = useQuotes;
        }

        public BalancingGroupNode(string balancedGroupName, string name, bool useQuotes)
            : this(balancedGroupName, useQuotes)
        {
            Name = name;
        }

        public BalancingGroupNode(string balancedGroupName, bool useQuotes, RegexNode childNode)
            : base(childNode)
        {
            BalancedGroupName = balancedGroupName;
            UseQuotes = useQuotes;
        }

        public BalancingGroupNode(string balancedGroupName, string name, bool useQuotes, RegexNode childNode)
            : this(balancedGroupName, useQuotes, childNode)
        {
            Name = name;
        }

        public BalancingGroupNode(string balancedGroupName, bool useQuotes, IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        {
            BalancedGroupName = balancedGroupName;
            UseQuotes = useQuotes;
        }

        public BalancingGroupNode(string balancedGroupName, string name, bool useQuotes, IEnumerable<RegexNode> childNodes)
            : this(balancedGroupName, useQuotes, childNodes)
        {
            Name = name;
        }

        protected override RegexNode CopyInstance()
        {
            return new BalancingGroupNode(BalancedGroupName, Name, UseQuotes);
        }

        public override string ToString()
        {
            return $"{Prefix}(?{(UseQuotes ? "'" : "<")}{(string.IsNullOrEmpty(Name) ? "" : $"{Name}")}-{BalancedGroupName}{(UseQuotes ? "'" : ">")}{string.Concat(ChildNodes)})";
        }
    }
}
