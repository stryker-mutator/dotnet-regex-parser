using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class AlternationNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToStringSeperatedByPipes()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            AlternationNode target = new(childNodes);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("a|b|c");
        }

        [TestMethod]
        public void SpanLengthShouldBeEqualToToStringLength()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            AlternationNode target = new(childNodes);

            // Act
            (int _, int Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(5);
        }

        [TestMethod]
        public void FirstAlternateSpanShouldStartBe0()
        {
            // Arrange
            CharacterNode firstChild = new('a');
            CharacterNode secondChild = new('b');
            CharacterNode thirdChild = new('c');
            List<RegexNode> childNodes = [firstChild, secondChild, thirdChild];
            _ = new AlternationNode(childNodes);

            // Act
            (int Start, int Length) = firstChild.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void NonFirstAlternateSpanShouldStartAfterPreviousAlternateAndPipe()
        {
            // Arrange
            CharacterNode firstChild = new('a');
            CharacterNode secondChild = new('b');
            CharacterNode thirdChild = new('c');
            List<RegexNode> childNodes = [firstChild, secondChild, thirdChild];
            _ = new AlternationNode(childNodes);

            // Act
            (int secondChildStart, int secondChildLength) = secondChild.GetSpan();
            (int thirdChildStart, int thirdChildLength) = thirdChild.GetSpan();

            // Assert
            secondChildStart.ShouldBe(2);
            secondChildLength.ShouldBe(1);
            thirdChildStart.ShouldBe(4);
            thirdChildLength.ShouldBe(1);
        }
    }
}
