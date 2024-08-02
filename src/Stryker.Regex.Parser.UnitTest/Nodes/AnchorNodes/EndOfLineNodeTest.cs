using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes.AnchorNodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class EndOfLineNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnDollarSign()
        {
            // Arrange
            var target = new EndOfLineNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("$");
        }

        [TestMethod]
        public void ToStringOnEndOfLineNodeWithPrefixShouldReturnCommentBeforeDollarSign()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new EndOfLineNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)$");
        }
    }
}
