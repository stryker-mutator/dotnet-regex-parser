using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class NamedGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnNamedGroupNodeWithUseQuotesFalseShouldReturnNamedGroupWithNameBetweenBrackets()
        {

            // Arrange
            var target = new NamedGroupNode("name", false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>)");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithUseQuotesTrueShouldReturnNamedGroupWithNameBetweenSingleQuotes()
        {

            // Arrange
            var target = new NamedGroupNode("name", true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?'name')");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithChildNodeShouldReturnNamedGroupWithChildNode()
        {

            // Arrange
            var childNode = new CharacterNode('a');
            var target = new NamedGroupNode("name", false, childNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>a)");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithMultipleChildNodesShouldReturnNamedGroupWithChildNodes()
        {

            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new NamedGroupNode("name", false, childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>abc)");
        }

        [TestMethod]
        public void CopyingNamedGroupNodeShouldCopyNameAndUseQuotes()
        {
            // Arrange
            var target = new NamedGroupNode("name", true);

            // Act
            // AddNode returns a copy of the current node.
            var result = target.AddNode(new CharacterNode('a'));

            // Assert
            NamedGroupNode namedGroupNode = result.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe(target.Name);
            namedGroupNode.UseQuotes.ShouldBe(target.UseQuotes);
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithprefixShouldReturnPrefixBeforeNamedGroupNode()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new NamedGroupNode("name", false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?<name>)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtNameLengthPlus4()
        {
            // Arrange
            var name = "name";
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new NamedGroupNode(name, false, childNodes);
            var start = name.Length + 4;

            // Act
            var (Start, Length) = target.ChildNodes.First().GetSpan();
            var (Start2, Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            var (Start3, _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(start);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
