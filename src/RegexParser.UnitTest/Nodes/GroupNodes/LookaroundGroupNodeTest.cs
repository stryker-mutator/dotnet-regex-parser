using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class LookaroundGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveTrueShouldReturnPossitiveLookahead()
        {

            // Arrange
            var target = new LookaroundGroupNode(true, true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?=)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadFalseAndPossitiveTrueShouldReturnPossitiveLookbehind()
        {

            // Arrange
            var target = new LookaroundGroupNode(false, true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<=)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveFalseShouldReturnNegativeLookahead()
        {

            // Arrange
            var target = new LookaroundGroupNode(true, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?!)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadFalseAndPossitiveFalseShouldReturnPossitiveLookahead()
        {

            // Arrange
            var target = new LookaroundGroupNode(false, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<!)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupChildNodeShouldReturnLookaroundWithChildNode()
        {

            // Arrange
            var childNode = new CharacterNode('a');
            var target = new LookaroundGroupNode(true, true, childNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?=a)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveTruedShouldReturnPossitiveLookahead()
        {

            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new LookaroundGroupNode(true, true, childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?=abc)");
        }

        [TestMethod]
        public void CopyingLookaroundGroupNodeShouldCopyLookaheadAndPossitive()
        {
            // Arrange
            var target = new LookaroundGroupNode(true, true);

            // Act
            // AddNode returns a copy of the current node.
            var result = target.AddNode(new CharacterNode('a'));

            // Assert
            LookaroundGroupNode lookaroundGroupNode = result.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBe(target.Lookahead);
            lookaroundGroupNode.Possitive.ShouldBe(target.Possitive);
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupNodeWithprefixShouldReturnPrefixBeforeLookaroundGroupNode()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new LookaroundGroupNode(true, true) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?=)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt3IfLookaheadIsFalse()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new LookaroundGroupNode(false, false, childNodes);

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(3);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt4IfLookaheadIsTrue()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new LookaroundGroupNode(true, false, childNodes);

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(4);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
