using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class EndOfStringZNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashUppercaseZ()
        {
            // Arrange
            var target = new EndOfStringZNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\Z");
        }

        [TestMethod]
        public void ToStringOnEndOfStringZNodeWithPrefixShouldReturnCommentBeforeBackslashUppercaseZ()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new EndOfStringZNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe($@"(?#This is a comment.)\Z");
        }
    }
}
