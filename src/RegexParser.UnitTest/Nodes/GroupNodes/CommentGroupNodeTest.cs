using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.CharacterClass;
using RegexParser.Nodes.GroupNodes;
using RegexParser.Nodes.QuantifierNodes;
using Shouldly;
using System;
using System.Collections.Generic;

namespace RegexParser.UnitTest.Nodes.GroupNodes
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
            var (Start, Length) = target.GetSpan();

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
            var (firstCommentStart, firstCommentLength) = firstComment.GetSpan();
            var (secondCommentStart, secondCommentLength) = target.GetSpan();

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
            var a = new CharacterNode('a');
            var b = new CharacterNode('b') { Prefix = comment };
            var concat = new ConcatenationNode(new List<RegexNode> { a, b });

            // Act
            var (commentStart, commentLength) = comment.GetSpan();
            var (aStart, aLength) = a.GetSpan();
            var (bStart, bLength) = b.GetSpan();
            var (concatStart, concatLength) = concat.GetSpan();

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
