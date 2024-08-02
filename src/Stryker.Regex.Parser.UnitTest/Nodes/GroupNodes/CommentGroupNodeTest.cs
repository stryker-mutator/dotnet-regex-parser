using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class CommentGroupNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnQuestionMarkHashtagCommentBetweenParentheses()
        {
            // Arrange
            var comment = "This is a comment.";
            var target = new CommentGroupNode(comment);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe($"(?#{comment})");
        }

        [TestMethod]
        public void ToStringOnEmptyCommentGroupShouldReturnQuestionMarkHashtagBetweenParentheses()
        {
            // Arrange
            var target = new CommentGroupNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#)");
        }

        [TestMethod]
        public void ToStringOnCommentGroupWithprefixShouldReturnPrefixBeforeCommentGroup()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new CommentGroupNode("This is the target.") { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?#This is the target.)");
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStartEqualToPrefixLengthAndLengthEqualToToStringLength()
        {
            // Arrange
            var target = new CommentGroupNode("This is a comment.");

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(target.ToString().Length);
        }

        [TestMethod]
        public void GetSpanOnNestedCommentGroupShouldReturnTupleWithStartEqualToPrefixLengthAndLengthEqualToToStringLentgthMinusPrefixLength()
        {
            // Arrange
            var firstComment = new CommentGroupNode("This is the first comment.");
            var target = new CommentGroupNode("This is the second comment.") { Prefix = firstComment };
            _ = new CharacterNode('a') { Prefix = target };

            // Act
            (var firstCommentStart, var firstCommentLength) = firstComment.GetSpan();
            (var secondCommentStart, var secondCommentLength) = target.GetSpan();

            // Assert
            firstCommentStart.ShouldBe(0);
            firstCommentLength.ShouldBe(firstComment.ToString().Length);
            secondCommentStart.ShouldBe(firstCommentStart + firstCommentLength);
            secondCommentLength.ShouldBe(target.ToString().Length - target.Prefix.ToString().Length);
        }

        [TestMethod]
        public void SpanOnCommentGroupShouldStartAfter()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            CharacterNode a = new('a');
            CharacterNode b = new('b') { Prefix = comment };
            ConcatenationNode concat = new([a, b]);

            // Act
            (var commentStart, var commentLength) = comment.GetSpan();
            (var aStart, var aLength) = a.GetSpan();
            (var bStart, var bLength) = b.GetSpan();
            (var concatStart, var concatLength) = concat.GetSpan();

            // Assert
            concatStart.ShouldBe(0);
            concatLength.ShouldBe(a.ToString().Length + b.ToString().Length);
            aStart.ShouldBe(0);
            aLength.ShouldBe(1);
            commentStart.ShouldBe(1);
            commentLength.ShouldBe(comment.ToString().Length);
            bStart.ShouldBe(commentStart + commentLength);
            bLength.ShouldBe(1);
        }
    }
}
