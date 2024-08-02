using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class EmptyNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnEmptyString()
        {
            // Arrange
            var target = new EmptyNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("");
        }

        [TestMethod]
        public void ToStringOnEmptyNodeWithPrefixShouldReturnPrefixToString()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new EmptyNode() { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)");
        }
    }
}
