using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;

namespace Stryker.Regex.Parser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassRangeNodeTest
    {
        [TestMethod]
        public void ToStringOnCharacterClassRangeNodeShouldReturnRangeWithStartAndEndSeperatedByDash()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            var target = new CharacterClassRangeNode(start, end);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a-z");
        }

        [TestMethod]
        public void StarttShouldReturnOriginalStart()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            var target = new CharacterClassRangeNode(start, end);

            // Act
            var result = target.Start;

            // Assert
            result.ShouldBe(start);
        }

        [TestMethod]
        public void EndShouldReturnOriginalEnd()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            var target = new CharacterClassRangeNode(start, end);

            // Act
            var result = target.End;

            // Assert
            result.ShouldBe(end);
        }

        [TestMethod]
        public void SpanLengthShouldBeEqualToToStringLength()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            var target = new CharacterClassRangeNode(start, end);

            // Act
            var (_, Length) = target.GetSpan();

            // Assert
            Length.ShouldBe(3);
        }

        [TestMethod]
        public void StartSpanShouldStartBe0()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            _ = new CharacterClassRangeNode(start, end);

            // Act
            var (Start, Length) = start.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(1);
        }

        [TestMethod]
        public void EndSpanShouldStartAfterStartAndDash()
        {
            // Arrange
            var start = new CharacterNode('a');
            var end = new CharacterNode('z');
            _ = new CharacterClassRangeNode(start, end);

            // Act
            var (Start, Length) = end.GetSpan();

            // Assert
            Start.ShouldBe(2);
            Length.ShouldBe(1);
        }
    }
}
