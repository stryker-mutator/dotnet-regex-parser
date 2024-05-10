using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes.AnchorNodes
{
    [TestClass]
    public class ContiguousMatchNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnBackslashUppercaseG()
        {
            // Arrange
            var target = new ContiguousMatchNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\G");
        }

        [TestMethod]
        public void ToStringOnContiguousMatchNodeWithPrefixShouldReturnCommentBeforeBackslashUppercaseG()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new ContiguousMatchNode() { Prefix = comment};

            // Act
            var result = target.ToString();
            var (Start, Length) = target.GetSpan();

            Start.ShouldBe(22);
            Length.ShouldBe(2);

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\G");
        }
    }
}
