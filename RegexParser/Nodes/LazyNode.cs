﻿using System.Linq;

namespace RegexParser.Nodes
{
    public class LazyNode : RegexNode
    {
        protected override int ChildSpanOffset => 0 - (ChildNodes.FirstOrDefault()?.ToString().Length ?? 0) - (Prefix?.ToString().Length ?? 0);

        private LazyNode()
        {
        }

        public LazyNode(RegexNode childNode)
            : base(childNode)
        {
        }

        protected override int GetSpanStart()
        {
            return base.GetSpanStart() + ChildNodes.FirstOrDefault()?.ToString().Length ?? 0;
        }

        protected override int GetSpanLength()
        {
            return base.GetSpanLength() - ChildNodes.FirstOrDefault()?.ToString().Length ?? 0;
        }

        public override string ToString()
        {
            return $"{ChildNodes.FirstOrDefault()}{Prefix}?";
        }
    }
}
