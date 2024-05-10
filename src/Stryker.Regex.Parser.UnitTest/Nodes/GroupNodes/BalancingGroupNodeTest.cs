using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes.GroupNodes
{
    [TestClass]
    public class BalancingGroupNodeTest
    {
        [TestMethod]
        public void ToStringOnBalancingGroupWithUseQuotesIsFalseShouldReturnBalancingGroupWithNameBetweenBrackets()
        {
            // Arrange
            BalancingGroupNode target = new("balancedGroup", "currentGroup", false);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>)");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithUseQuotesIsTrueShouldReturnBalancingGroupWithNameBetweenBrackets()
        {
            // Arrange
            BalancingGroupNode target = new("balancedGroup", "currentGroup", true);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?'currentGroup-balancedGroup')");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithChildNodeShouldReturnBalencingGroupWithChildNode()
        {
            // Arrange
            CharacterNode childNode = new('a');
            BalancingGroupNode target = new("balancedGroup", "currentGroup", false, childNode);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>a)");
        }

        [TestMethod]
        public void ToStringOnBalancingGroupWithMultipleChildNodesShouldReturnBalencingGroupWithChildNodes()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            BalancingGroupNode target = new("balancedGroup", "currentGroup", false, childNodes);

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?<currentGroup-balancedGroup>abc)");
        }

        [TestMethod]
        public void CopyingBalancingGroupNodeShouldCopyBalancedGroupNameAndUseQuotes()
        {
            // Arrange
            BalancingGroupNode target = new("balancedGroup", "currentGroup", true);

            // Act
            // AddNode returns a copy of the current node.
            RegexNode result = target.AddNode(new CharacterNode('a'));

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
            CommentGroupNode comment = new("This is a comment.");
            BalancingGroupNode target = new("balancedGroup", "currentGroup", false) { Prefix = comment };

            // Act
            string result = target.ToString();

            // Assert
            result.ShouldBe("(?#This is a comment.)(?<currentGroup-balancedGroup>)");
        }

        [TestMethod]
        public void ChildNodesGetSpanShouldReturnTupleWithStartEqualToPreviousChildsStartPlusLengthStartingAtFullNameLengthPlus5()
        {
            // Arrange
            string balancedName = "balancedName";
            string name = "name";
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
            BalancingGroupNode target = new(balancedName, name, false, childNodes);
            int start = balancedName.Length + name.Length + 5;

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
