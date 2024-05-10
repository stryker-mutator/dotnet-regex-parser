using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class StartOfStringNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashUppercaseA()
        {
            // Arrange
            var target = new StartOfStringNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\A");
        }

        [TestMethod]
        public void ToStringOnStartOfStringNodeWithPrefixShouldReturnCommentBeforeBackslashUppercaseA()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new StartOfStringNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\A");
        }
    }
}
