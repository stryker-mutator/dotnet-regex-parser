using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;

namespace Stryker.Regex.Parser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassNodeTest
    {
        [TestMethod]
        public void ToStringOnCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("[abc]");
        }

        [TestMethod]
        public void ToStringOnNegatedCharacterClassNodeWithCharacterSetShouldReturnCharactersBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("[^abc]");
        }

        [TestMethod]
        public void ToStringOnCharacterClassNodeWithSubtractionSubtractionBetweenBrackets()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, subtraction, false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("[abc-[a]]");
        }

        [TestMethod]
        public void CopyingCharacterClassNodeShouldCopyNegation()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassCharacterSetNode replacementCharacterSet = new(new List<RegexNode> { new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, true);

            // Act
            // ReplaceNode returns a copy of the current node.
            RegexNode result = target.ReplaceNode(characterSet, replacementCharacterSet);

            // Assert
            CharacterClassNode characterClassNode = result.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBe(target.Negated);
        }

        [TestMethod]
        public void CharacterSetShouldReturnOriginalCharacterSet()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, false);

            // Act
            CharacterClassCharacterSetNode result = target.CharacterSet;

            // Assert
            result.ShouldBe(characterSet);
        }

        [TestMethod]
        public void SubtractionShouldReturnOriginalSubtraction()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, subtraction, false);

            // Act
            CharacterClassNode result = target.Subtraction;

            // Assert
            result.ShouldBe(subtraction);
        }

        [TestMethod]
        public void SubtractionShouldReturnNullIfNoSubtraction()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, false);

            // Act
            CharacterClassNode result = target.Subtraction;

            // Assert
            result.ShouldBeNull();
        }

        [TestMethod]
        public void ToStringOnCharacterClassNodeWithPrefixShouldReturnPrefixBeforeCharacterClass()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            CharacterClassNode target = new(characterSet, false) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)[abc]");
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt1fNegatedIsFalse()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, false);

            // Act
            (int Start, int _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(1);
        }

        [TestMethod]
        public void CharacterSetSpanShouldStartAt2IfNegatedIsTrue()
        {
            // Arrange
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, true);

            // Act
            (int Start, int _) = characterSet.GetSpan();

            // Assert
            Start.ShouldBe(2);
        }

        [TestMethod]
        public void SubtractionSpanShouldStartAfterCharacterSetAndDash()
        {
            // Arrange
            CharacterClassCharacterSetNode subtractionCharacterSet = new(new CharacterNode('a'));
            CharacterClassNode subtraction = new(subtractionCharacterSet, false);
            CharacterClassCharacterSetNode characterSet = new(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') });
            _ = new CharacterClassNode(characterSet, subtraction, false);

            // Act
            (int Start, int _) = subtraction.GetSpan();

            // Assert
            Start.ShouldBe(5);
        }
    }
}
