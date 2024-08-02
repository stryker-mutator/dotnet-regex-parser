using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class ConcatenationNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToString()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            ConcatenationNode target = new(childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("abc");
        }

        [TestMethod]
        public void ToStringOnEmptyNodeShouldReturnEmptyString()
        {
            // Arrange
            ConcatenationNode target = new();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("");
        }
        [TestMethod]
        public void SpanLengthOfEmptyConcatenationNodeShouldBeEqualToToStringLength()
        {
            // Arrange
            ConcatenationNode target = new();

            // Act
            (var _, var Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOConcatenationNodeWithChildNodesShouldBeEqualToToStringLength()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            ConcatenationNode target = new(childNodes);

            // Act
            (var _, var Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfConcatenationNodeShouldNotIncludeItsOwnPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("Comment");
            ConcatenationNode target = new() { Prefix = prefix };

            // Act
            (var _, var Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length - prefix.ToString().Length);
        }

        [TestMethod]
        public void SpanLengthOfConcatenationNodeShouldIncludeChildNodesPrefix()
        {
            // Arrange
            CharacterNode childNode = new('a') { Prefix = new CommentGroupNode("Comment") };
            ConcatenationNode target = new(childNode);

            // Act
            (var _, var Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt0()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            ConcatenationNode target = new(childNodes);

            // Act
            (var Start, var Length) = target.ChildNodes.First().GetSpan();
            (var Start2, var Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (var Start3, var _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(0);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
