using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class CharacterNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnCharAsString()
        {
            // Arrange
            var target = new CharacterNode('a');

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a");
        }

        [TestMethod]
        public void CopyingCharacterNodeShouldCopyOriginalCharacter()
        {
            // Arrange
            var target = new CharacterNode('a');

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            CharacterNode characterNode = result.ShouldBeOfType<CharacterNode>();
            characterNode.Character.ShouldBe(target.Character);
        }

        [TestMethod]
        public void ToStringOnCharacterNodeWithPrefixShouldReturnPrefixBeforeCharacter()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new CharacterNode('a') { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)a");
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStart0AndLenght1()
        {
            // Arrange
            var target = new CharacterNode('a');

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStartEqualToPrefixLengthAndLength1()
        {
            // Arrange
            var comment = new CommentGroupNode("X");
            var target = new CharacterNode('a') { Prefix = comment };

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(5);
            Length.ShouldBe(1);
        }
    }
}
