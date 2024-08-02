using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.QuantifierNodes
{
    [TestClass]
    public class QuantifierQuestionMarkNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnQuantifierQuestionMarkOnChildNodeToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            var target = new QuantifierQuestionMarkNode(characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a?");
        }

        [TestMethod]
        public void ToStringOnQuantifierWithPrefixShouldReturnPrefixBeforeOriginalQuantifierAndAfterQuantifiersChildNode()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            var target = new QuantifierQuestionMarkNode(characterNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a(?#This is a comment.)?");
        }

        [TestMethod]
        public void SpanShouldStartAfterChildNodes()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new QuantifierQuestionMarkNode(childNode);

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
            var target = new QuantifierQuestionMarkNode(childNode) { Prefix = prefix };

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
            _ = new QuantifierQuestionMarkNode(target);

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
            _ = new QuantifierQuestionMarkNode(target) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }
    }
}
