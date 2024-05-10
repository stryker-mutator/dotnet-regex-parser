using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class UnicodeCategoryNodeTest
    {

        [TestMethod]
        public void ToStringWithNegatedFalseShouldReturnBackslashLowercasePWithCategoryBetweenCurlyBrackets()
        {
            // Arrange
            var target = new UnicodeCategoryNode("IsBasicLatin", false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\p{IsBasicLatin}");
        }

        [TestMethod]
        public void ToStringWithNegatedTrueShouldReturnBackslashUppercasePWithCategoryBetweenCurlyBrackets()
        {
            // Arrange
            var target = new UnicodeCategoryNode("IsBasicLatin", true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"\P{IsBasicLatin}");
        }

        [TestMethod]
        public void CopyingUnicodeCategoryNodeShouldCopyOriginalCategoryAndNegated()
        {
            // Arrange
            var target = new UnicodeCategoryNode("IsBasicLatin", true);

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            UnicodeCategoryNode unicodeCategoryNode = result.ShouldBeOfType<UnicodeCategoryNode>();
            unicodeCategoryNode.Category.ShouldBe(target.Category);
            unicodeCategoryNode.Negated.ShouldBe(target.Negated);
        }

        [TestMethod]
        public void ToStringOnUnicodeCategoryNodeWithPrefixShouldReturnPrefixBeforeUnicodeCategory()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new UnicodeCategoryNode("IsBasicLatin", false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\p{IsBasicLatin}");
        }
    }
}
