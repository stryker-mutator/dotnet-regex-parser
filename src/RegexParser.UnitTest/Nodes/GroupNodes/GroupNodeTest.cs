using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    public class TestGroupNode : GroupNode
    {
        public TestGroupNode()
        {
        }

        public TestGroupNode(RegexNode childNode)
            : base(childNode)
        {
        }

        public TestGroupNode(IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        {
        }

        public override string ToString()
        {
            return string.Concat(ChildNodes);
        }
    }

    [TestClass]
    public class GroupNodeTest
    {

        [TestMethod]
        public void SpanLengthOfEmptyGroupShouldBeEqualToToStringLength()
        {
            // Arrange
            var target = new TestGroupNode();

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupWithChildNodesShouldBeEqualToToStringLength()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new TestGroupNode(childNodes);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupShouldNotIncludeItsOwnPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("Comment");
            var target = new TestGroupNode() { Prefix = prefix };

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length - prefix.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupShouldIncludeChildNodesPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a') { Prefix = new CommentGroupNode("Comment") };
            var target = new TestGroupNode(childNode);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }
    }
}
