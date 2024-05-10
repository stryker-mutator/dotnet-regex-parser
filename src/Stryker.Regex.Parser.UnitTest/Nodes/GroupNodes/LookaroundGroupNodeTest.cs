using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class LookaroundGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveTrueShouldReturnPossitiveLookahead()
        {

            // Arrange
            LookaroundGroupNode target = new(true, true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?=)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadFalseAndPossitiveTrueShouldReturnPossitiveLookbehind()
        {

            // Arrange
            LookaroundGroupNode target = new(false, true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<=)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveFalseShouldReturnNegativeLookahead()
        {

            // Arrange
            LookaroundGroupNode target = new(true, false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?!)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadFalseAndPossitiveFalseShouldReturnPossitiveLookahead()
        {

            // Arrange
            LookaroundGroupNode target = new(false, false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<!)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupChildNodeShouldReturnLookaroundWithChildNode()
        {

            // Arrange
            CharacterNode childNode = new('a');
            LookaroundGroupNode target = new(true, true, childNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?=a)");
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupWithLookaheadTrueAndPossitiveTruedShouldReturnPossitiveLookahead()
        {

            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            LookaroundGroupNode target = new(true, true, childNodes);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?=abc)");
        }

        [TestMethod]
        public void CopyingLookaroundGroupNodeShouldCopyLookaheadAndPossitive()
        {
            // Arrange
            LookaroundGroupNode target = new(true, true);

            // Act
            // AddNode returns a copy of the current node.
            RegexNode result = target.AddNode(new CharacterNode('a'));

            // Assert
            LookaroundGroupNode lookaroundGroupNode = result.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBe(target.Lookahead);
            lookaroundGroupNode.Possitive.ShouldBe(target.Possitive);
        }

        [TestMethod]
        public void ToStringOnLookaroundGroupNodeWithprefixShouldReturnPrefixBeforeLookaroundGroupNode()
        {

            // Arrange
            CommentGroupNode comment = new("This is a comment.");
            LookaroundGroupNode target = new(true, true) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?=)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt3IfLookaheadIsFalse()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            LookaroundGroupNode target = new(false, false, childNodes);

            // Act
            (int Start, int Length) = target.ChildNodes.First().GetSpan();
            (int Start2, int Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (int Start3, int _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(3);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAt4IfLookaheadIsTrue()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            LookaroundGroupNode target = new(true, false, childNodes);

            // Act
            (int Start, int Length) = target.ChildNodes.First().GetSpan();
            (int Start2, int Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (int Start3, int _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(4);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
