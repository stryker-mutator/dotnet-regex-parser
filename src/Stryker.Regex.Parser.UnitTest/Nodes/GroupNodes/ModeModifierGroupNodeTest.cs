using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class ModeModifierGroupNodeTest
    {

        [TestMethod]
        public void ToStringOnEmptyModeModifierGroupNodeShouldReturnModeModifierGroupWithModifiers()
        {

            // Arrange
            ModeModifierGroupNode target = new("imsnx-imsnx");

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx)");
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithChildNodeShouldReturnModeModifierGroupWithChildNodeAfterColon()
        {

            // Arrange
            CharacterNode childNode = new('a');
            ModeModifierGroupNode target = new("imsnx-imsnx", childNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx:a)");
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithMultipleChildNodesShouldReturnModeModifierGroupWithChildNodesAfterColon()
        {

            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            ModeModifierGroupNode target = new("imsnx-imsnx", childNodes);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?imsnx-imsnx:abc)");
        }

        [TestMethod]
        public void CopyingModeModifierGroupNodeShouldCopyModifiers()
        {
            // Arrange
            ModeModifierGroupNode target = new("imsnx-imsnx");

            // Act
            // AddNode returns a copy of the current node.
            RegexNode result = target.AddNode(new CharacterNode('a'));

            // Assert
            ModeModifierGroupNode modeModifierGroupNode = result.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe(target.Modifiers);
        }

        [TestMethod]
        public void ToStringOnModeModifierGroupNodeWithprefixShouldReturnPrefixBeforeModeModifierGroupNode()
        {

            // Arrange
            CommentGroupNode comment = new("This is a comment.");
            ModeModifierGroupNode target = new("imsnx-imsnx") { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?imsnx-imsnx)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtModesLengthPlus3()
        {
            // Arrange
            string modes = "imsnx-imsnx";
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            ModeModifierGroupNode target = new(modes, childNodes);
            int start = modes.Length + 3;

            // Act
            (int Start, int Length) = target.ChildNodes.First().GetSpan();
            (int Start2, int Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (int Start3, int _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(start);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
