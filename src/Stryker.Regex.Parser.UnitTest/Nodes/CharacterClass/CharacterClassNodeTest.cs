using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassNodeTest
    {
        [TestMethod]
        public void ToStringOnCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[abc]");
        }

        [TestMethod]
        public void ToStringOnNegatedCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[^abc]");
        }

        [TestMethod]
        public void ToStringOnCharacterClassNodeWithSubtractionSubtractionBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, subtraction, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[abc-[a]]");
        }

        [TestMethod]
        public void CopyingCharacterClassNodeShouldCopyNegation()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassCharacterSetNode replacementCharacterSet = new([new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, true);

            // Act
            // ReplaceNode returns a copy of the current node.
            var result = target.ReplaceNode(characterSet, replacementCharacterSet);

            // Assert
            var characterClassNode = result.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBe(target.Negated);
        }

        [TestMethod]
        public void CharacterSetShouldReturnOriginalCharacterSet()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, false);

            // Act
            var result = target.CharacterSet;

            // Assert
            result.ShouldBe(characterSet);
        }

        [TestMethod]
        public void SubtractionShouldReturnOriginalSubtraction()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, subtraction, false);

            // Act
            var result = target.Subtraction;

            // Assert
            result.ShouldBe(subtraction);
        }

        [TestMethod]
        public void SubtractionShouldReturnNullIfNoSubtraction()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, false);

            // Act
            var result = target.Subtraction;

            // Assert
            result.ShouldBeNull();
        }

        [TestMethod]
        public void ToStringOnCharacterClassNodeWithPrefixShouldReturnPrefixBeforeCharacterClass()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            CharacterClassNode target = new(characterSet, false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)[abc]");
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt1fNegatedIsFalse()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            _ = new CharacterClassNode(characterSet, false);

            // Act
            (var Start, var _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(1);
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt2IfNegatedIsTrue()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            _ = new CharacterClassNode(characterSet, true);

            // Act
            (var Start, var _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(2);
        }

        [TestMethod]
        public void SubtractionSpanShouldStartAfterCharacterSetAndDash()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new([new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')]);
            _ = new CharacterClassNode(characterSet, subtraction, false);

            // Act
            (var Start, var _) = subtraction.GetSpan();

            // Assert
            Start.ShouldBe(5);
        }
    }
}
