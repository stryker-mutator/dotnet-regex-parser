using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.QuantifierNodes
{
    [TestClass]
    public class QuantifierNOrMoreNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnOriginalQuantifierNOrMoreOnChildNodeToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNOrMoreNode target = new("05", characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a{05,}");
        }


        [TestMethod]
        public void ToStringShouldReturnQuantifierNOrMoreOfIntegerNIfNoOriginalNIsGiven()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNOrMoreNode target = new(5, characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a{5,}");
        }

        [TestMethod]
        public void CopyingQuantifierNOrMoreNodeShouldCopyOriginalNAndN()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNOrMoreNode target = new("5", childNode);

            // Act
            // ReplaceNode returns a copy of the current node.
            var result = target.ReplaceNode(childNode, new CharacterNode('b'));

            // Assert
            var quantifierNorMoreNode = result.ShouldBeOfType<QuantifierNOrMoreNode>();
            quantifierNorMoreNode.OriginalN.ShouldBe(target.OriginalN);
            quantifierNorMoreNode.N.ShouldBe(target.N);
        }

        [TestMethod]
        public void ToStringOnQuantifierWithPrefixShouldReturnPrefixBeforeOriginalQuantifierAndAfterQuantifiersChildNode()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            QuantifierNOrMoreNode target = new("05", characterNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a(?#This is a comment.){05,}");
        }

        [TestMethod]
        public void SpanShouldStartAfterChildNodes()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNOrMoreNode target = new(5, childNode);

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(childNode.ToString().Length);
            Length.ShouldBe(4);
        }

        [TestMethod]
        public void SpanShouldStartAfterPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var prefix = new CommentGroupNode("X");
            QuantifierNOrMoreNode target = new(5, childNode) { Prefix = prefix };

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(6);
            Length.ShouldBe(4);
        }

        [TestMethod]
        public void ChildNodeShouldStartBeforeQuantifier()
        {
            // Arrange
            var target = new CharacterNode('a');
            _ = new QuantifierNOrMoreNode(5, target);

            // Act
            (var Start, var Length) = target.GetSpan();

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
            _ = new QuantifierNOrMoreNode(5, target) { Prefix = prefix };

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }
    }
}
