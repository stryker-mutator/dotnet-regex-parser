using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class ModeModifierGroupNodeTest
    {

        [TestMethod]
        public void ToStringOnEmptyModeModifierGroupNodeShouldReturnModeModifierGroupWithModifiers()
        {

            // Arrange
            var target = new ModeModifierGroupNode("imsnx-imsnx");

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx)");
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithChildNodeShouldReturnModeModifierGroupWithChildNodeAfterColon()
        {

            // Arrange
            var childNode = new CharacterNode('a');
            var target = new ModeModifierGroupNode("imsnx-imsnx", childNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx:a)");
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithMultipleChildNodesShouldReturnModeModifierGroupWithChildNodesAfterColon()
        {

            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new ModeModifierGroupNode("imsnx-imsnx", childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx:abc)");
        }

        [TestMethod]
        public void CopyingModeModifierGroupNodeShouldCopyModifiers()
        {
            // Arrange
            var target = new ModeModifierGroupNode("imsnx-imsnx");

            // Act
            // AddNode returns a copy of the current node.
            var result = target.AddNode(new CharacterNode('a'));

            // Assert
            ModeModifierGroupNode modeModifierGroupNode = result.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe(target.Modifiers);
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithprefixShouldReturnPrefixBeforeModeModifierGroupNode()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new ModeModifierGroupNode("imsnx-imsnx") { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?imsnx-imsnx)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtModesLengthPlus3()
        {
            // Arrange
            var modes = "imsnx-imsnx";
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new ModeModifierGroupNode(modes, childNodes);
            var start = modes.Length + 3;

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(start);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
