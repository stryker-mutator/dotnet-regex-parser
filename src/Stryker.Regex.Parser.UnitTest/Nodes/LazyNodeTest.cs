using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class LazyNodeTest
    {
        [TestMethod]
        public void ToStringShouldAppendQuestionMarkToChildToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            var quantifierNode = new QuantifierStarNode(characterNode);
            var target = new LazyNode(quantifierNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a*?");
        }

        [TestMethod]
        public void ToStringOnLazyNodeWithPrefixShouldReturnPrefixBeforeLazyNodeAndAfterQuantifier()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            var quantifierNode = new QuantifierStarNode(characterNode);
            var target = new LazyNode(quantifierNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a*(?#This is a comment.)?");
        }

        [TestMethod]
        public void SpanShouldStartAfterQuantifier()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var quantifier = new QuantifierStarNode(childNode);
            var target = new LazyNode(quantifier);

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(2);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void SpanShouldStartAfterPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var quantifier = new QuantifierStarNode(childNode);
            var prefix = new CommentGroupNode("X");
            var target = new LazyNode(quantifier) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(7);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void QuantifierShouldStartBeforeLazyToken()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new QuantifierStarNode(childNode);
            _ = new LazyNode(target);

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(1);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void QuantifierShouldStartBeforeLazyTokensPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new QuantifierStarNode(childNode);
            var prefix = new CommentGroupNode("X");
            _ = new LazyNode(target) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(1);
            Length.ShouldBe(1);
        }
    }
}
