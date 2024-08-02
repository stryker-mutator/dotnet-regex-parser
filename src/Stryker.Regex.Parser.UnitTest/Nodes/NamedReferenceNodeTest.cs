using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class NamedReferenceNodeTest
    {
        [TestMethod]
        public void NamedReferenceWithUseQuotesTrueToStringShouldReturnBackslashLowercaseKNameBetweenSingleQuotes()
        {
            // Arrange
            NamedReferenceNode target = new("name", true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\k'name'");
        }

        [TestMethod]
        public void NamedReferenceWithUseQuotesFalseToStringShouldReturnBackslashLowercaseKNameBetweenBrackets()
        {
            // Arrange
            NamedReferenceNode target = new("name", false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\k<name>");
        }

        [TestMethod]
        public void NamedReferenceWithUseKFalseToStringShouldReturnBackslashNameBetweenBrackets()
        {
            // Arrange
            NamedReferenceNode target = new("name", false, false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\<name>");
        }


        [TestMethod]
        public void CopyingNamedReferenceNodeShouldCopyOriginalNameUseQuotesAndUseK()
        {
            // Arrange
            NamedReferenceNode target = new("name", true, true);

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            var namedReferenceNode = result.ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(target.Name);
            namedReferenceNode.UseQuotes.ShouldBe(target.UseQuotes);
            namedReferenceNode.UseK.ShouldBe(target.UseK);
        }

        [TestMethod]
        public void ToStringOnNamedReferenceNodeWithPrefixShouldReturnPrefixBeforeNamedReference()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            NamedReferenceNode target = new("name", false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\k<name>");
        }
    }
}
