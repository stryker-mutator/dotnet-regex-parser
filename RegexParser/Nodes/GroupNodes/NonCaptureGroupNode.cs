﻿using System.Collections.Generic;

namespace RegexParser.Nodes.GroupNodes
{
    public class NonCaptureGroupNode : GroupNode
    {
        protected override int ChildSpanOffset => 3;

        public NonCaptureGroupNode()
        {
        }

        public NonCaptureGroupNode(RegexNode childNode)
            : base(childNode)
        {
        }

        public NonCaptureGroupNode(IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        {
        }

        public override string ToString()
        {
            return $"{Prefix}(?:{string.Concat(ChildNodes)})";
        }
    }
}
