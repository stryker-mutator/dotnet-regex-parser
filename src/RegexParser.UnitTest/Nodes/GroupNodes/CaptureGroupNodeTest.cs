using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class CaptureGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnEmptyNodeShouldReturnEmptyCaptureGroup()
        {

            // Arrange
            var target = new CaptureGroupNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("()");
        }

        [TestMethod]
        public void ToStringOnCaptureGroupNodeWithChildNodeCaptureGroupWithChildNode()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new CaptureGroupNode(childNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(a)");
        }

        [TestMethod]
        public void ToStringOnCaptureGroupNodeMulitpleWithChildNodesCaptureGroupWithChildNodes()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new CaptureGroupNode(childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(abc)");
        }

        [TestMethod]
        public void ToStringOnCaptureGroupNodeWithprefixShouldReturnPrefixBeforeCaptureGroupNode()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var childNode = new CharacterNode('a');
            var target = new CaptureGroupNode(childNode) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(a)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt1()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new CaptureGroupNode(childNodes);

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(1);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
