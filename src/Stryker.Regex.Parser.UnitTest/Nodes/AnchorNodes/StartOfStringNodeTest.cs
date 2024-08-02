using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes.AnchorNodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.AnchorNodes
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
