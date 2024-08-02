using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.QuantifierNodes
{
    [TestClass]
    public class QuantifierNNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnOriginalQuantifierNOnChildNodeToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNNode target = new("05", characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a{05}");
        }


        [TestMethod]
        public void ToStringShouldReturnQuantifierNOfIntegerNIfNoOriginalNIsGiven()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNNode target = new(5, characterNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a{5}");
        }

        [TestMethod]
        public void CopyingQuantifierNNodeShouldCopyOriginalNAndN()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNNode target = new("5", childNode);

            // Act
            // ReplaceNode returns a copy of the current node.
            var result = target.ReplaceNode(childNode, new CharacterNode('b'));

            // Assert
            var quantifierNNode = result.ShouldBeOfType<QuantifierNNode>();
            quantifierNNode.OriginalN.ShouldBe(target.OriginalN);
            quantifierNNode.N.ShouldBe(target.N);
        }

        [TestMethod]
        public void ToStringOnQuantifierWithPrefixShouldReturnPrefixBeforeOriginalQuantifierAndAfterQuantifiersChildNode()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            QuantifierNNode target = new("05", characterNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a(?#This is a comment.){05}");
        }

        [TestMethod]
        public void SpanShouldStartAfterChildNodes()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNNode target = new(5, childNode);

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(childNode.ToString().Length);
            Length.ShouldBe(3);
        }

        [TestMethod]
        public void SpanShouldStartAfterPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var prefix = new CommentGroupNode("X");
            QuantifierNNode target = new(5, childNode) { Prefix = prefix };

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(6);
            Length.ShouldBe(3);
        }

        [TestMethod]
        public void ChildNodeShouldStartBeforeQuantifier()
        {
            // Arrange
            var target = new CharacterNode('a');
            _ = new QuantifierNNode(5, target);

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
            _ = new QuantifierNNode(5, target) { Prefix = prefix };

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }
    }
}
