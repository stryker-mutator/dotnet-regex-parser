using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.QuantifierNodes
{
    [TestClass]
    public class QuantifierStarNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnQuantifierStarOnChildNodeToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            var target = new QuantifierStarNode(characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a*");
        }

        [TestMethod]
        public void ToStringOnQuantifierWithPrefixShouldReturnPrefixBeforeOriginalQuantifierAndAfterQuantifiersChildNode()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            var target = new QuantifierStarNode(characterNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a(?#This is a comment.)*");
        }

        [TestMethod]
        public void SpanShouldStartAfterChildNodes()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new QuantifierStarNode(childNode);

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(childNode.ToString().Length);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void SpanShouldStartAfterPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var prefix = new CommentGroupNode("X");
            var target = new QuantifierStarNode(childNode) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(6);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void ChildNodeShouldStartBeforeQuantifier()
        {
            // Arrange
            var target = new CharacterNode('a');
            _ = new QuantifierStarNode(target);

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void ChildNodeShouldStartBeforeQuantifiersPrefix()
        {
            // Arrange
            var target = new CharacterNode('a');
            var prefix = new CommentGroupNode("X");
            _ = new QuantifierStarNode(target) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }
    }
}
