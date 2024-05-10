using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class ConcatenationNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToString()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new ConcatenationNode(childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("abc");
        }

        [TestMethod]
        public void ToStringOnEmptyNodeShouldReturnEmptyString()
        {
            // Arrange
            var target = new ConcatenationNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("");
        }
        [TestMethod]
        public void SpanLengthOfEmptyConcatenationNodeShouldBeEqualToToStringLength()
        {
            // Arrange
            var target = new ConcatenationNode();

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOConcatenationNodeWithChildNodesShouldBeEqualToToStringLength()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new ConcatenationNode(childNodes);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfConcatenationNodeShouldNotIncludeItsOwnPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("Comment");
            var target = new ConcatenationNode() { Prefix = prefix };

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length - prefix.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfConcatenationNodeShouldIncludeChildNodesPrefix()
        {
            // Arrange
            var childNode = new CharacterNode('a') { Prefix = new CommentGroupNode("Comment") };
            var target = new ConcatenationNode(childNode);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt0()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new ConcatenationNode(childNodes);

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(0);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
