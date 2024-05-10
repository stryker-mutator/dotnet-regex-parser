using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.CharacterClass;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;

namespace RegexParser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassNodeTest
    {
        [TestMethod]
        public void ToStringOnCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[abc]");
        }

        [TestMethod]
        public void ToStringOnNegatedCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[^abc]");
        }

        [TestMethod]
        public void ToStringOnCharacterClassNodeWithSubtractionSubtractionBetweenBrackets()
        {
            // Arrange
            var subtractionCharacterSet = new CharacterClassCharacterSetNode(new CharacterNode('a'));
            var subtraction = new CharacterClassNode(subtractionCharacterSet, false);
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, subtraction, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("[abc-[a]]");
        }

        [TestMethod]
        public void CopyingCharacterClassNodeShouldCopyNegation()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var replacementCharacterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, true);

            // Act
            // ReplaceNode returns a copy of the current node.
            var result = target.ReplaceNode(characterSet, replacementCharacterSet);

            // Assert
            CharacterClassNode characterClassNode = result.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBe(target.Negated);
        }

        [TestMethod]
        public void CharacterSetShouldReturnOriginalCharacterSet()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, false);

            // Act
            var result = target.CharacterSet;

            // Assert
            result.ShouldBe(characterSet);
        }

        [TestMethod]
        public void SubtractionShouldReturnOriginalSubtraction()
        {
            // Arrange
            var subtractionCharacterSet = new CharacterClassCharacterSetNode(new CharacterNode('a'));
            var subtraction = new CharacterClassNode(subtractionCharacterSet, false);
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, subtraction, false);

            // Act
            var result = target.Subtraction;

            // Assert
            result.ShouldBe(subtraction);
        }

        [TestMethod]
        public void SubtractionShouldReturnNullIfNoSubtraction()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, false);

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
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            var target = new CharacterClassNode(characterSet, false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)[abc]");
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt1fNegatedIsFalse()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, false);

            // Act
            var (Start, _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(1);
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt2IfNegatedIsTrue()
        {
            // Arrange
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, true);

            // Act
            var (Start, _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(2);
        }

        [TestMethod]
        public void SubtractionSpanShouldStartAfterCharacterSetAndDash()
        {
            // Arrange
            var subtractionCharacterSet = new CharacterClassCharacterSetNode(new CharacterNode('a'));
            var subtraction = new CharacterClassNode(subtractionCharacterSet, false);
            var characterSet = new CharacterClassCharacterSetNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, subtraction, false);

            // Act
            var (Start, _) = subtraction.GetSpan();

            // Assert
            Start.ShouldBe(5);
        }
    }
}
