using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.QuantifierNodes
{
    [TestClass]
    public class QuantifierNMNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnOriginalQuantifierNMOnChildNodeToString()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNMNode target = new("05", "006", characterNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("a{05,006}");
        }

        [TestMethod]
        public void ToStringShouldReturnQuantifierNMOfIntegersNAndMIfNoOriginalNAndMIsGiven()
        {
            // Arrange
            var characterNode = new CharacterNode('a');
            QuantifierNMNode target = new(5, 6, characterNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("a{5,6}");
        }

        [TestMethod]
        public void CopyingQuantifierNMNodeShouldCopyOriginalNOringalMAndNAndM()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNMNode target = new("5", "10", childNode);

            // Act
            // ReplaceNode returns a copy of the current node.
            Regex.Parser.Nodes.RegexNode result = target.ReplaceNode(childNode, new CharacterNode('b'));

            // Assert
            QuantifierNMNode quantifierNMNode = result.ShouldBeOfType<QuantifierNMNode>();
            quantifierNMNode.OriginalN.ShouldBe(target.OriginalN);
            quantifierNMNode.N.ShouldBe(target.N);
            quantifierNMNode.OriginalM.ShouldBe(target.OriginalM);
            quantifierNMNode.M.ShouldBe(target.M);
        }

        [TestMethod]
        public void ToStringOnQuantifierWithPrefixShouldReturnPrefixBeforeOriginalQuantifierAndAfterQuantifiersChildNode()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var characterNode = new CharacterNode('a');
            QuantifierNMNode target = new("05", "006", characterNode) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("a(?#This is a comment.){05,006}");
        }

        [TestMethod]
        public void SpanShouldStartAfterChildNodes()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            QuantifierNMNode target = new(5, 10, childNode);

            // Act
            (int Start, int Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(childNode.ToString().Length);
            Length.ShouldBe(6);
        }

        [TestMethod]
        public void SpanShouldStartAfterPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var prefix = new CommentGroupNode("X");
            QuantifierNMNode target = new(5, 10, childNode) { Prefix = prefix };

            // Act
            (int Start, int Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(6);
            Length.ShouldBe(6);
        }

        [TestMethod]
        public void ChildNodeShouldStartBeforeQuantifier()
        {
            // Arrange
            var target = new CharacterNode('a');
            _ = new QuantifierNMNode(5, 10, target);

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
            _ = new QuantifierNMNode(5, 10, target) { Prefix = prefix };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }
    }
}
