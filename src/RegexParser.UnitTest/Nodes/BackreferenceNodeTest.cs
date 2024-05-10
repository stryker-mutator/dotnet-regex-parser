using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class BackreferenceNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashGroupNumber()
        {
            // Arrange
            var target = new BackreferenceNode(5);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\5");
        }

        [TestMethod]
        public void CopyingBackreferenceNodeShouldCopyOriginalGroupNumberAndGroupNumber()
        {
            // Arrange
            var target = new BackreferenceNode(5);

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('a'));

            // Assert
            BackreferenceNode backreferenceNode = result.ShouldBeOfType<BackreferenceNode>();
            backreferenceNode.GroupNumber.ShouldBe(target.GroupNumber);
        }

        [TestMethod]
        public void ToStringOnBackreferenceNodeWithPrefixShouldReturnPrefixBeforeBackreference()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new BackreferenceNode(5) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\5");
        }
    }
}
