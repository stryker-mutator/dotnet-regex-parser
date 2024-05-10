using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class StartOfLineNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnCaret()
        {
            // Arrange
            var target = new StartOfLineNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("^");
        }

        [TestMethod]
        public void ToStringOnStartOfLineNodeWithPrefixShouldReturnCommentBeforeCaret()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new StartOfLineNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)^");
        }
    }
}
