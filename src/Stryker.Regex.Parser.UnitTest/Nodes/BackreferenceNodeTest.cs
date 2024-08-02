using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class BackreferenceNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashGroupNumber()
        {
            // Arrange
            BackreferenceNode target = new(5);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\5");
        }

        [TestMethod]
        public void CopyingBackreferenceNodeShouldCopyOriginalGroupNumberAndGroupNumber()
        {
            // Arrange
            BackreferenceNode target = new(5);

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('a'));

            // Assert
            var backreferenceNode = result.ShouldBeOfType<BackreferenceNode>();
            backreferenceNode.GroupNumber.ShouldBe(target.GroupNumber);
        }

        [TestMethod]
        public void ToStringOnBackreferenceNodeWithPrefixShouldReturnPrefixBeforeBackreference()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            BackreferenceNode target = new(5) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\5");
        }
    }
}
