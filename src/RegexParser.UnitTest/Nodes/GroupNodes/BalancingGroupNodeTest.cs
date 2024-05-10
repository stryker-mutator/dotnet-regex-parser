using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class BalancingGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnBalancingGroupWithUseQuotesIsFalseShouldReturnBalancingGroupWithNameBetweenBrackets()
        {
            // Arrange
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", false);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>)");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithUseQuotesIsTrueShouldReturnBalancingGroupWithNameBetweenBrackets()
        {
            // Arrange
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", true);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?'currentGroup-balancedGroup')");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithChildNodeShouldReturnBalencingGroupWithChildNode()
        {
            // Arrange
            var childNode = new CharacterNode('a');
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", false, childNode);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>a)");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithMultipleChildNodesShouldReturnBalencingGroupWithChildNodes()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", false, childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>abc)");
        }

        [TestMethod]
        public void CopyingBalancingGroupNodeShouldCopyBalancedGroupNameAndUseQuotes()
        {
            // Arrange
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", true);

            // Act
            // AddNode returns a copy of the current node.
            var result = target.AddNode(new CharacterNode('a'));

            // Assert
            BalancingGroupNode balancingGroupNode = result.ShouldBeOfType<BalancingGroupNode>();
            balancingGroupNode.BalancedGroupName.ShouldBe(target.BalancedGroupName);
            balancingGroupNode.Name.ShouldBe(target.Name);
            balancingGroupNode.UseQuotes.ShouldBe(target.UseQuotes);
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithPrefixShouldReturnPrefixBeforeBalancingGroup()
        {

            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = new BalancingGroupNode("balancedGroup", "currentGroup", false) { Prefix = comment };

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?<currentGroup-balancedGroup>)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtFullNameLengthPlus5()
        {
            // Arrange
            var balancedName = "balancedName";
            var name = "name";
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new BalancingGroupNode(balancedName, name, false, childNodes);
            var start = balancedName.Length + name.Length + 5;

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
