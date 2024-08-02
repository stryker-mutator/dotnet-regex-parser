using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes.AnchorNodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class WordBoundaryNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashLowercaseB()
        {
            // Arrange
            var target = new WordBoundaryNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\b");
        }

        [TestMethod]
        public void ToStringOnWordBoundaryNodeWithPrefixShouldReturnCommentBeforeBackslashLowercaseB()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new WordBoundaryNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\b");
        }
    }
}
