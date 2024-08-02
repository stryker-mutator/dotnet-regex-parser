using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
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
            TestGroupNode target = new();

            // Act
            (int _, int Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupWithChildNodesShouldBeEqualToToStringLength()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            TestGroupNode target = new(childNodes);

            // Act
            (int _, int Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupShouldNotIncludeItsOwnPrefix()
        {
            // Arrange
            CommentGroupNode prefix = new("Comment");
            TestGroupNode target = new() { Prefix = prefix };

            // Act
            (int _, int Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length - prefix.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfGroupShouldIncludeChildNodesPrefix()
        {
            // Arrange
            CharacterNode childNode = new('a') { Prefix = new CommentGroupNode("Comment") };
            TestGroupNode target = new(childNode);

            // Act
            (int _, int Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }
    }
}
