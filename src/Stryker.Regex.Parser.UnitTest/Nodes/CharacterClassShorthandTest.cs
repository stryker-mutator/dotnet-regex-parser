using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class CharacterClassShorthandTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashShorthand()
        {
            // Arrange
            CharacterClassShorthandNode target = new('d');

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\d");
        }

        [TestMethod]
        public void CopyingCharacterClassShorthandNodeShouldCopyShorthand()
        {
            // Arrange
            CharacterClassShorthandNode target = new('d');

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('a'));

            // Assert
            var characterClassShorthandNode = result.ShouldBeOfType<CharacterClassShorthandNode>();
            characterClassShorthandNode.Shorthand.ShouldBe(target.Shorthand);
        }

        [TestMethod]
        public void ToStringOnCharacterClassShorthandNodeWithPrefixShouldReturnPrefixBeforeCharacterClassShorthand()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            CharacterClassShorthandNode target = new('d') { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\d");
        }
    }
}
