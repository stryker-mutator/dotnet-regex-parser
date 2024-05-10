using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using Shouldly;
using System.Collections.Generic;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class AlternationNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToStringSeperatedByPipes()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new AlternationNode(childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a|b|c");
        }

        [TestMethod]
        public void SpanLengthShouldBeEqualToToStringLength()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new AlternationNode(childNodes);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(5);
        }

        [TestMethod]
        public void FirstAlternateSpanShouldStartBe0()
        {
            // Arrange
            var firstChild = new CharacterNode('a');
            var secondChild = new CharacterNode('b');
            var thirdChild = new CharacterNode('c');
            var childNodes = new List<RegexNode> { firstChild, secondChild, thirdChild };
            _ = new AlternationNode(childNodes);

            // Act
            var (Start, Length) = firstChild.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void NonFirstAlternateSpanShouldStartAfterPreviousAlternateAndPipe()
        {
            // Arrange
            var firstChild = new CharacterNode('a');
            var secondChild = new CharacterNode('b');
            var thirdChild = new CharacterNode('c');
            var childNodes = new List<RegexNode> { firstChild, secondChild, thirdChild };
            _ = new AlternationNode(childNodes);

            // Act
            var (secondChildStart, secondChildLength) = secondChild.GetSpan();
            var (thirdChildStart, thirdChildLength) = thirdChild.GetSpan();

            // Assert
            secondChildStart.ShouldBe(2);
            secondChildLength.ShouldBe(1);
            thirdChildStart.ShouldBe(4);
            thirdChildLength.ShouldBe(1);
        }
    }
}
