using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;

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
            string result = target.ToString();

            // Assert
            result.ShouldBe("abc");
        }

        [TestMethod]
        public void ToStringOnEmptyNodeShouldReturnEmptyString()
        {
            // Arrange
            ConcatenationNode target = new();

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("");
        }
        [TestMethod]
        public void SpanLengthOfEmptyConcatenationNodeShouldBeEqualToToStringLength()
        {
            // Arrange
            ConcatenationNode target = new();

            // Act
            (int _, int Length) = target.GetSpan();

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
            (int _, int Length) = target.GetSpan();

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
            (int _, int Length) = target.GetSpan();

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
            (int _, int Length) = target.GetSpan();

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
            (int Start, int Length) = target.ChildNodes.First().GetSpan();
            (int Start2, int Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (int Start3, int _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(0);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
