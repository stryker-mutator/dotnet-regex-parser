using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class NamedGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnNamedGroupNodeWithUseQuotesFalseShouldReturnNamedGroupWithNameBetweenBrackets()
        {

            // Arrange
            NamedGroupNode target = new("name", false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>)");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithUseQuotesTrueShouldReturnNamedGroupWithNameBetweenSingleQuotes()
        {

            // Arrange
            NamedGroupNode target = new("name", true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?'name')");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithChildNodeShouldReturnNamedGroupWithChildNode()
        {

            // Arrange
            CharacterNode childNode = new('a');
            NamedGroupNode target = new("name", false, childNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>a)");
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithMultipleChildNodesShouldReturnNamedGroupWithChildNodes()
        {

            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            NamedGroupNode target = new("name", false, childNodes);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<name>abc)");
        }

        [TestMethod]
        public void CopyingNamedGroupNodeShouldCopyNameAndUseQuotes()
        {
            // Arrange
            NamedGroupNode target = new("name", true);

            // Act
            // AddNode returns a copy of the current node.
            RegexNode result = target.AddNode(new CharacterNode('a'));

            // Assert
            NamedGroupNode namedGroupNode = result.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe(target.Name);
            namedGroupNode.UseQuotes.ShouldBe(target.UseQuotes);
        }

        [TestMethod]
        public void ToStringOnNamedGroupNodeWithprefixShouldReturnPrefixBeforeNamedGroupNode()
        {

            // Arrange
            CommentGroupNode comment = new("This is a comment.");
            NamedGroupNode target = new("name", false) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?<name>)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtNameLengthPlus4()
        {
            // Arrange
            string name = "name";
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            NamedGroupNode target = new(name, false, childNodes);
            int start = name.Length + 4;

            // Act
            (int Start, int Length) = target.ChildNodes.First().GetSpan();
            (int Start2, int Length2) = target.ChildNodes.ElementAt(1).GetSpan();
            (int Start3, int _) = target.ChildNodes.ElementAt(2).GetSpan();

            // Assert
            Start.ShouldBe(start);
            Start2.ShouldBe(Start + Length);
            Start3.ShouldBe(Start2 + Length2);
        }
    }
}
