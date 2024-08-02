using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class CharacterNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnCharAsString()
        {
            // Arrange
            CharacterNode target = new('a');

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a");
        }

        [TestMethod]
        public void CopyingCharacterNodeShouldCopyOriginalCharacter()
        {
            // Arrange
            CharacterNode target = new('a');

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            var characterNode = result.ShouldBeOfType<CharacterNode>();
            characterNode.Character.ShouldBe(target.Character);
        }

        [TestMethod]
        public void ToStringOnCharacterNodeWithPrefixShouldReturnPrefixBeforeCharacter()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            CharacterNode target = new('a') { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)a");
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStart0AndLenght1()
        {
            // Arrange
            CharacterNode target = new('a');

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStartEqualToPrefixLengthAndLength1()
        {
            // Arrange
            var comment = new CommentGroupNode("X");
            CharacterNode target = new('a') { Prefix = comment };

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(5);
            Length.ShouldBe(1);
        }
    }
}
