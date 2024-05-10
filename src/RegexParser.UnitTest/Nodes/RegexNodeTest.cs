using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.GroupNodes;
using RegexParser.Nodes.QuantifierNodes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class RegexNodeTest
    {
        [TestMethod]
        public void EmptyConstructorShouldReturnNewRegexNodeWithNoChildNodes()
        {
            // Arrange
            var target = new TestRegexNode();

            // Assert
            target.ChildNodes.ShouldBeEmpty();
        }

        [TestMethod]
        public void ConstructorWithChildNodesShouldReturnNewRegexNodeWithChildNodes()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b') };

            // Act
            var target = new TestRegexNode(childNodes);

            // Assert
            target.ChildNodes.Count().ShouldBe(2);
            target.ChildNodes.First().ToString().ShouldBe("a");
            target.ChildNodes.ElementAt(1).ToString().ShouldBe("b");
        }

        [TestMethod]
        public void AddNodeShouldCopyNodeAndAddNewRegexNode()
        {
            // Arrange
            var target = new TestRegexNode(new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b') });
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.Last().ShouldBe(newNode);
        }

        [TestMethod]
        public void AddNodeShouldCopyDescendants()
        {
            // Arrange
            var grandChildConcatNode = new ConcatenationNode(new List<RegexNode> { new CharacterNode('d'), new CharacterNode('e') });
            var childConcatNode = new ConcatenationNode(new List<RegexNode> { grandChildConcatNode, new CharacterNode('a'), new CharacterNode('b') });
            var target = new TestRegexNode(childConcatNode);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.First().ChildNodes.First().ChildNodes.Count().ShouldBe(2);
        }

        [TestMethod]
        public void AddNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var childNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(childNodes);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void AddNodeShouldReturnRootNode()
        {
            // Arrange
            var targetChildNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b') };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            var copiedTargetParent = result.ChildNodes.ShouldHaveSingleItem();
            var modifiedTarget = copiedTargetParent.ChildNodes.ShouldHaveSingleItem();
            modifiedTarget.ChildNodes.Count().ShouldBe(3);
            modifiedTarget.ChildNodes.Last().ShouldBe(newNode);
        }

        [TestMethod]
        public void AddNodeShouldNotReturnRootNodeIfReturnRootIsFalse()
        {
            // Arrange
            var targetChildNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b') };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.AddNode(newNode, false);

            // Assert
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.Last().ShouldBe(newNode);
        }

        [TestMethod]
        public void ReplaceNodeShouldCopyNodeAndReplaceOldNodeWithNewNode()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var childNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(childNodes);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.ReplaceNode(charNodeA, newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ShouldBe(newNode);
        }

        [TestMethod]
        public void ReplaceNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var childNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(childNodes);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.ReplaceNode(charNodeA, newNode);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void ReplaceNodeShouldReturnRootNode()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var targetChildNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.ReplaceNode(charNodeA, newNode);

            // Assert
            var copiedTargetParent = result.ChildNodes.ShouldHaveSingleItem();
            var modifierTarget = copiedTargetParent.ChildNodes.ShouldHaveSingleItem();
            modifierTarget.ChildNodes.Count().ShouldBe(2);
            modifierTarget.ChildNodes.First().ShouldBe(newNode);
        }

        [TestMethod]
        public void ReplaceNodeShouldNotReturnRootNodeIfReturnRootIsFalse()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var targetChildNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);
            var newNode = new CharacterNode('c');

            // Act
            RegexNode result = target.ReplaceNode(charNodeA, newNode, false);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ShouldBe(newNode);
        }

        [TestMethod]
        public void RemoveNodeShouldCopyNodeAndRemoveOldNode()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var childNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(childNodes);

            // Act
            RegexNode result = target.RemoveNode(charNodeA);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ToString().ShouldBe("b");
        }

        [TestMethod]
        public void RemoveNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var childNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(childNodes);

            // Act
            RegexNode result = target.RemoveNode(charNodeA);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void RemoveNodeShouldReturnRootNode()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var targetChildNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);

            // Act
            RegexNode result = target.RemoveNode(charNodeA);

            // Assert
            var copiedTargetParentNode = result.ChildNodes.ShouldHaveSingleItem();
            var modifiedNode = copiedTargetParentNode.ChildNodes.ShouldHaveSingleItem();
            modifiedNode.ChildNodes.ShouldHaveSingleItem();
            modifiedNode.ChildNodes.First().ToString().ShouldBe("b");
        }

        [TestMethod]
        public void RemoveNodeShouldNotReturnRootNodeIfReturnRootIsFalse()
        {
            // Arrange
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var targetChildNodes = new List<RegexNode> { charNodeA, charNodeB };
            var target = new TestRegexNode(targetChildNodes);
            var targetParent = new TestRegexNode(target);
            _ = new TestRegexNode(targetParent);

            // Act
            RegexNode result = target.RemoveNode(charNodeA, false);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ToString().ShouldBe("b");
        }

        [TestMethod]
        public void GetDescendantsShouldReturnAllDescendants()
        {
            // Arrange
            // a+bc*
            var charNodeA = new CharacterNode('a');
            var charNodeB = new CharacterNode('b');
            var charNodeC = new CharacterNode('c');
            var quantifierPlus = new QuantifierPlusNode(charNodeA);
            var quantifierStar = new QuantifierStarNode(charNodeC);
            var grandchildren = new List<RegexNode> { quantifierPlus, charNodeB, quantifierStar };
            var concatenationNode = new ConcatenationNode(grandchildren);
            var target = new TestRegexNode(concatenationNode);

            // Act
            IEnumerable<RegexNode> result = target.GetDescendantNodes();

            // Assert
            result.Count().ShouldBe(6);
            result.First().ShouldBe(charNodeA);
            result.ElementAt(1).ShouldBe(quantifierPlus);
            result.ElementAt(2).ShouldBe(charNodeB);
            result.ElementAt(3).ShouldBe(charNodeC);
            result.ElementAt(4).ShouldBe(quantifierStar);
            result.Last().ShouldBe(concatenationNode);
        }

        [TestMethod]
        public void GetDescendantsOnNodeWithNoChildrenShouldReturnEmptyIEnumerable()
        {
            // Arrange
            var target = new TestRegexNode();

            // Act
            IEnumerable<RegexNode> result = target.GetDescendantNodes();

            // Assert
            result.ShouldBeEmpty();
        }

        [TestMethod]
        public void AddNodeShouldCopyPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var target = new TestRegexNode { Prefix = prefix };
            var newNode = new TestRegexNode();

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void AddNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var target = new TestRegexNode { Prefix = prefix };
            var newNode = new TestRegexNode();

            // Act
            RegexNode result = target.AddNode(newNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void RemoveNodeShouldCopyRootsPrefix()
        {
            // Arrange
            var prefix2 = new CommentGroupNode("This is the prefix's prefix.");
            var prefix = new CommentGroupNode("This is a prefix.") { Prefix = prefix2 };
            var oldNode = new TestRegexNode();
            var target = new TestRegexNode(oldNode) { Prefix = prefix };

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void RemoveNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode();
            var target = new TestRegexNode(oldNode) { Prefix = prefix };

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void RemoveNodeShouldMoveOldNodesPrefixToNextNodeInTheCopy()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = prefix };
            var nextNode = new TestRegexNode();
            var target = new ConcatenationNode(new List<RegexNode> { oldNode, nextNode });

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            var remainingNode = result.ChildNodes.ShouldHaveSingleItem();
            remainingNode.Prefix.ShouldNotBeNull();
            remainingNode.Prefix.Comment.ShouldBe(prefix.Comment);
        }

        [TestMethod]
        public void RemoveNodeShouldNotChangeOriginalNextNodeWhenMovingPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = prefix };
            var nextNode = new TestRegexNode();
            var target = new ConcatenationNode(new List<RegexNode> { oldNode, nextNode });

            // Act
            _ = target.RemoveNode(oldNode);

            // Assert
            nextNode.Prefix.ShouldBeNull();
        }

        [TestMethod]
        public void RemoveNodeShouldAddOldNodesPrefixAsPrefixToNextNodesPrefixIfNextNodeAlreadyHasPrefix()
        {
            // Arrange
            var oldNodePrefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = oldNodePrefix };
            var nextNodePrefix = new CommentGroupNode("This is the prefix of the next node.");
            var nextNode = new TestRegexNode() { Prefix = nextNodePrefix };
            var target = new ConcatenationNode(new List<RegexNode> { oldNode, nextNode });

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            var remainingNode = result.ChildNodes.ShouldHaveSingleItem();
            remainingNode.Prefix.Comment.ShouldBe(nextNode.Prefix.Comment);
            remainingNode.Prefix.Prefix.ShouldNotBeNull();
            remainingNode.Prefix.Prefix.Comment.ShouldBe(oldNodePrefix.Comment);
        }

        [TestMethod]
        public void RemoveNodeShouldAddOldNodesPrefixAsFirstPrefixIfNextNodeAlreadyHasMultiplePrefixes()
        {
            // Arrange
            var oldNodePrefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = oldNodePrefix };
            var nextNodeFirstPrefix = new CommentGroupNode("This is the first prefix of the next node.");
            var nextNodeSecondPrefix = new CommentGroupNode("This is the second prefix of the next node.") { Prefix = nextNodeFirstPrefix };
            var nextNode = new TestRegexNode() { Prefix = nextNodeSecondPrefix };
            var target = new ConcatenationNode(new List<RegexNode> { oldNode, nextNode });

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            var remainingNode = result.ChildNodes.ShouldHaveSingleItem();
            remainingNode.Prefix.Comment.ShouldBe(nextNode.Prefix.Comment);
            remainingNode.Prefix.Prefix.ShouldNotBeNull();
            remainingNode.Prefix.Prefix.Comment.ShouldBe(nextNode.Prefix.Prefix.Comment);
            remainingNode.Prefix.Prefix.Prefix.ShouldNotBeNull();
            remainingNode.Prefix.Prefix.Prefix.Comment.ShouldBe(oldNodePrefix.Comment);
        }

        [TestMethod]
        public void RemoveNodeShouldAddEmptyNodeWithOldNodesPrefixIfThereAreNoNextNodes()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = prefix };
            var target = new ConcatenationNode(new List<RegexNode> { oldNode });

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            var emptyNode = result.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EmptyNode>();
            emptyNode.Prefix.ShouldNotBeNull();
            emptyNode.Prefix.Comment.ShouldBe(prefix.Comment);
        }

        [TestMethod]
        public void RemoveNodeShouldNotAddEmptyNodeWithIfOldNodeHasNoPrefix()
        {
            // Arrange
            var oldNode = new TestRegexNode();
            var target = new ConcatenationNode(new List<RegexNode> { oldNode });

            // Act
            RegexNode result = target.RemoveNode(oldNode);

            // Assert
            result.ChildNodes.ShouldBeEmpty();
        }

        [TestMethod]
        public void ReplaceNodeShouldCopyPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode();
            var newNode = new TestRegexNode();
            var target = new TestRegexNode(oldNode) { Prefix = prefix };

            // Act
            RegexNode result = target.ReplaceNode(oldNode, newNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void ReplaceNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode();
            var newNode = new TestRegexNode();
            var target = new TestRegexNode(oldNode) { Prefix = prefix };

            // Act
            RegexNode result = target.ReplaceNode(oldNode, newNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void ReplaceNodeShouldMoveOldNodesPrefixToNewNode()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = prefix };
            var newNode = new TestRegexNode();
            var target = new ConcatenationNode(oldNode);

            // Act
            RegexNode result = target.ReplaceNode(oldNode, newNode);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.Prefix.ShouldNotBeNull();
            childNode.Prefix.Comment.ShouldBe(prefix.Comment);
        }

        [TestMethod]
        public void ReplaceNodeShouldAddOldNodesPrefixAsPrefixToNewNodesPrefixIfNewNodeAlreadyHasPrefix()
        {
            // Arrange
            var oldNodePrefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = oldNodePrefix };
            var newNodePrefix = new CommentGroupNode("This is the prefix of the next node.");
            var newNode = new TestRegexNode() { Prefix = newNodePrefix };
            var target = new ConcatenationNode(oldNode);

            // Act
            RegexNode result = target.ReplaceNode(oldNode, newNode);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.Prefix.Comment.ShouldBe(newNode.Prefix.Comment);
            childNode.Prefix.Prefix.ShouldNotBeNull();
            childNode.Prefix.Prefix.Comment.ShouldBe(oldNodePrefix.Comment);
        }

        [TestMethod]
        public void ReplaceNodeShouldAddOldNodesPrefixAsFirstPrefixIfNewNodeAlreadyHasMultiplePrefixes()
        {
            // Arrange
            var oldNodePrefix = new CommentGroupNode("This is a prefix.");
            var oldNode = new TestRegexNode() { Prefix = oldNodePrefix };
            var newNodeFirstPrefix = new CommentGroupNode("This is the first prefix of the new node.");
            var newNodeSecondPrefix = new CommentGroupNode("This is the second prefix of the new node.") { Prefix = newNodeFirstPrefix };
            var newNode = new TestRegexNode() { Prefix = newNodeSecondPrefix };
            var target = new ConcatenationNode(oldNode);

            // Act
            RegexNode result = target.ReplaceNode(oldNode, newNode);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.Prefix.Comment.ShouldBe(newNode.Prefix.Comment);
            childNode.Prefix.Prefix.ShouldNotBeNull();
            childNode.Prefix.Prefix.Comment.ShouldBe(newNode.Prefix.Prefix.Comment);
            childNode.Prefix.Prefix.Prefix.ShouldNotBeNull();
            childNode.Prefix.Prefix.Prefix.Comment.ShouldBe(oldNodePrefix.Comment);
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStart0AndLenghtEqualToToStringLength()
        {
            // Arrange
            var target = new TestRegexNode();
            var targetLength = target.ToString().Length;

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(targetLength);
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStartEqualToPrefixLengthAndEqualToToStringLengthMinusPrefixLength()
        {
            // Arrange
            var commentGroup = new CommentGroupNode("X");
            var target = new TestRegexNode() { Prefix = commentGroup };
            var prefixLength = commentGroup.ToString().Length;
            var targetLength = target.ToString().Length;

            // Act
            var (Start, Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(prefixLength);
            Length.ShouldBe(targetLength - prefixLength);
        }
    }

    public class TestRegexNode : RegexNode
    {
        public TestRegexNode()
        {
        }

        public TestRegexNode(RegexNode childNode)
            : base(childNode)
        {
        }

        public TestRegexNode(IEnumerable<RegexNode> childNodes)
            : base(childNodes)
        {
        }

        public override string ToString()
        {
            return $"{Prefix}RegexTestNode";
        }
    }
}
