using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class UnicodeCategoryNodeTest
    {

        [TestMethod]
        public void ToStringWithNegatedFalseShouldReturnBackslashLowercasePWithCategoryBetweenCurlyBrackets()
        {
            // Arrange
            UnicodeCategoryNode target = new("IsBasicLatin", false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe(@"\p{IsBasicLatin}");
        }

        [TestMethod]
        public void ToStringWithNegatedTrueShouldReturnBackslashUppercasePWithCategoryBetweenCurlyBrackets()
        {
            // Arrange
            UnicodeCategoryNode target = new("IsBasicLatin", true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe(@"\P{IsBasicLatin}");
        }

        [TestMethod]
        public void CopyingUnicodeCategoryNodeShouldCopyOriginalCategoryAndNegated()
        {
            // Arrange
            UnicodeCategoryNode target = new("IsBasicLatin", true);

            // Act
            // RemoveNode returns a copy of the current node.
            RegexNode result = target.RemoveNode(new CharacterNode('x'));

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
            UnicodeCategoryNode target = new("IsBasicLatin", false) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\p{IsBasicLatin}");
        }
    }
}
