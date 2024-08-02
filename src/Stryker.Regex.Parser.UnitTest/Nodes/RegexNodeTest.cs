using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class RegexNodeTest
    {
        [TestMethod]
        public void EmptyConstructorShouldReturnNewRegexNodeWithNoChildNodes()
        {
            // Arrange
            TestRegexNode target = new();

            // Assert
            target.ChildNodes.ShouldBeEmpty();
        }

        [TestMethod]
        public void ConstructorWithChildNodesShouldReturnNewRegexNodeWithChildNodes()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b')];

            // Act
            TestRegexNode target = new(childNodes);

            // Assert
            target.ChildNodes.Count().ShouldBe(2);
            target.ChildNodes.First().ToString().ShouldBe("a");
            target.ChildNodes.ElementAt(1).ToString().ShouldBe("b");
        }

        [TestMethod]
        public void AddNodeShouldCopyNodeAndAddNewRegexNode()
        {
            // Arrange
            TestRegexNode target = new([new CharacterNode('a'), new CharacterNode('b')]);
            CharacterNode newNode = new('c');

            // Act
            var result = target.AddNode(newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.Last().ShouldBe(newNode);
        }

        [TestMethod]
        public void AddNodeShouldCopyDescendants()
        {
            // Arrange
            ConcatenationNode grandChildConcatNode = new([new CharacterNode('d'), new CharacterNode('e')]);
            ConcatenationNode childConcatNode = new([grandChildConcatNode, new CharacterNode('a'), new CharacterNode('b')]);
            TestRegexNode target = new(childConcatNode);
            CharacterNode newNode = new('c');

            // Act
            var result = target.AddNode(newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.First().ChildNodes.First().ChildNodes.Count().ShouldBe(2);
        }

        [TestMethod]
        public void AddNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> childNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(childNodes);
            CharacterNode newNode = new('c');

            // Act
            var result = target.AddNode(newNode);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void AddNodeShouldReturnRootNode()
        {
            // Arrange
            List<RegexNode> targetChildNodes = [new CharacterNode('a'), new CharacterNode('b')];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);
            CharacterNode newNode = new('c');

            // Act
            var result = target.AddNode(newNode);

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
            List<RegexNode> targetChildNodes = [new CharacterNode('a'), new CharacterNode('b')];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);
            CharacterNode newNode = new('c');

            // Act
            var result = target.AddNode(newNode, false);

            // Assert
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.Last().ShouldBe(newNode);
        }

        [TestMethod]
        public void ReplaceNodeShouldCopyNodeAndReplaceOldNodeWithNewNode()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> childNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(childNodes);
            CharacterNode newNode = new('c');

            // Act
            var result = target.ReplaceNode(charNodeA, newNode);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ShouldBe(newNode);
        }

        [TestMethod]
        public void ReplaceNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> childNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(childNodes);
            CharacterNode newNode = new('c');

            // Act
            var result = target.ReplaceNode(charNodeA, newNode);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void ReplaceNodeShouldReturnRootNode()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> targetChildNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);
            CharacterNode newNode = new('c');

            // Act
            var result = target.ReplaceNode(charNodeA, newNode);

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
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> targetChildNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);
            CharacterNode newNode = new('c');

            // Act
            var result = target.ReplaceNode(charNodeA, newNode, false);

            // Assert
            result.ChildNodes.Count().ShouldBe(2);
            result.ChildNodes.First().ShouldBe(newNode);
        }

        [TestMethod]
        public void RemoveNodeShouldCopyNodeAndRemoveOldNode()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> childNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(childNodes);

            // Act
            var result = target.RemoveNode(charNodeA);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ToString().ShouldBe("b");
        }

        [TestMethod]
        public void RemoveNodeShouldHaveNoReferencesToTheOriginalTreeNodes()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> childNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(childNodes);

            // Act
            var result = target.RemoveNode(charNodeA);

            // Assert
            result.ShouldNotBe(target);
            result.ChildNodes.ShouldNotContain(charNodeA);
            result.ChildNodes.ShouldNotContain(charNodeB);
        }

        [TestMethod]
        public void RemoveNodeShouldReturnRootNode()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> targetChildNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);

            // Act
            var result = target.RemoveNode(charNodeA);

            // Assert
            var copiedTargetParentNode = result.ChildNodes.ShouldHaveSingleItem();
            var modifiedNode = copiedTargetParentNode.ChildNodes.ShouldHaveSingleItem();
            _ = modifiedNode.ChildNodes.ShouldHaveSingleItem();
            modifiedNode.ChildNodes.First().ToString().ShouldBe("b");
        }

        [TestMethod]
        public void RemoveNodeShouldNotReturnRootNodeIfReturnRootIsFalse()
        {
            // Arrange
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            List<RegexNode> targetChildNodes = [charNodeA, charNodeB];
            TestRegexNode target = new(targetChildNodes);
            TestRegexNode targetParent = new(target);
            _ = new TestRegexNode(targetParent);

            // Act
            var result = target.RemoveNode(charNodeA, false);

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ToString().ShouldBe("b");
        }

        [TestMethod]
        public void GetDescendantsShouldReturnAllDescendants()
        {
            // Arrange
            // a+bc*
            CharacterNode charNodeA = new('a');
            CharacterNode charNodeB = new('b');
            CharacterNode charNodeC = new('c');
            var quantifierPlus = new QuantifierPlusNode(charNodeA);
            var quantifierStar = new QuantifierStarNode(charNodeC);
            List<RegexNode> grandchildren = [quantifierPlus, charNodeB, quantifierStar];
            ConcatenationNode concatenationNode = new(grandchildren);
            TestRegexNode target = new(concatenationNode);

            // Act
            var result = target.GetDescendantNodes();

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
            TestRegexNode target = new();

            // Act
            var result = target.GetDescendantNodes();

            // Assert
            result.ShouldBeEmpty();
        }

        [TestMethod]
        public void AddNodeShouldCopyPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode target = new() { Prefix = prefix };
            TestRegexNode newNode = new();

            // Act
            var result = target.AddNode(newNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void AddNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode target = new() { Prefix = prefix };
            TestRegexNode newNode = new();

            // Act
            var result = target.AddNode(newNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void RemoveNodeShouldCopyRootsPrefix()
        {
            // Arrange
            var prefix2 = new CommentGroupNode("This is the prefix's prefix.");
            var prefix = new CommentGroupNode("This is a prefix.") { Prefix = prefix2 };
            TestRegexNode oldNode = new();
            TestRegexNode target = new(oldNode) { Prefix = prefix };

            // Act
            var result = target.RemoveNode(oldNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void RemoveNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode oldNode = new();
            TestRegexNode target = new(oldNode) { Prefix = prefix };

            // Act
            var result = target.RemoveNode(oldNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void RemoveNodeShouldMoveOldNodesPrefixToNextNodeInTheCopy()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode oldNode = new() { Prefix = prefix };
            TestRegexNode nextNode = new();
            ConcatenationNode target = new([oldNode, nextNode]);

            // Act
            var result = target.RemoveNode(oldNode);

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
            TestRegexNode oldNode = new() { Prefix = prefix };
            TestRegexNode nextNode = new();
            ConcatenationNode target = new([oldNode, nextNode]);

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
            TestRegexNode oldNode = new() { Prefix = oldNodePrefix };
            var nextNodePrefix = new CommentGroupNode("This is the prefix of the next node.");
            TestRegexNode nextNode = new() { Prefix = nextNodePrefix };
            ConcatenationNode target = new([oldNode, nextNode]);

            // Act
            var result = target.RemoveNode(oldNode);

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
            TestRegexNode oldNode = new() { Prefix = oldNodePrefix };
            var nextNodeFirstPrefix = new CommentGroupNode("This is the first prefix of the next node.");
            var nextNodeSecondPrefix = new CommentGroupNode("This is the second prefix of the next node.") { Prefix = nextNodeFirstPrefix };
            TestRegexNode nextNode = new() { Prefix = nextNodeSecondPrefix };
            ConcatenationNode target = new([oldNode, nextNode]);

            // Act
            var result = target.RemoveNode(oldNode);

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
            TestRegexNode oldNode = new() { Prefix = prefix };
            ConcatenationNode target = new([oldNode]);

            // Act
            var result = target.RemoveNode(oldNode);

            // Assert
            var emptyNode = result.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EmptyNode>();
            emptyNode.Prefix.ShouldNotBeNull();
            emptyNode.Prefix.Comment.ShouldBe(prefix.Comment);
        }

        [TestMethod]
        public void RemoveNodeShouldNotAddEmptyNodeWithIfOldNodeHasNoPrefix()
        {
            // Arrange
            TestRegexNode oldNode = new();
            ConcatenationNode target = new([oldNode]);

            // Act
            var result = target.RemoveNode(oldNode);

            // Assert
            result.ChildNodes.ShouldBeEmpty();
        }

        [TestMethod]
        public void ReplaceNodeShouldCopyPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode oldNode = new();
            TestRegexNode newNode = new();
            TestRegexNode target = new(oldNode) { Prefix = prefix };

            // Act
            var result = target.ReplaceNode(oldNode, newNode);

            // Assert
            result.Prefix.ToString().ShouldBe(target.Prefix.ToString());
        }

        [TestMethod]
        public void ReplaceNodeResultShouldNotHaveReferenceToOriginalPrefix()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode oldNode = new();
            TestRegexNode newNode = new();
            TestRegexNode target = new(oldNode) { Prefix = prefix };

            // Act
            var result = target.ReplaceNode(oldNode, newNode);

            // Assert
            result.Prefix.ShouldNotBe(target.Prefix);
        }

        [TestMethod]
        public void ReplaceNodeShouldMoveOldNodesPrefixToNewNode()
        {
            // Arrange
            var prefix = new CommentGroupNode("This is a prefix.");
            TestRegexNode oldNode = new() { Prefix = prefix };
            TestRegexNode newNode = new();
            ConcatenationNode target = new(oldNode);

            // Act
            var result = target.ReplaceNode(oldNode, newNode);

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
            TestRegexNode oldNode = new() { Prefix = oldNodePrefix };
            var newNodePrefix = new CommentGroupNode("This is the prefix of the next node.");
            TestRegexNode newNode = new() { Prefix = newNodePrefix };
            ConcatenationNode target = new(oldNode);

            // Act
            var result = target.ReplaceNode(oldNode, newNode);

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
            TestRegexNode oldNode = new() { Prefix = oldNodePrefix };
            var newNodeFirstPrefix = new CommentGroupNode("This is the first prefix of the new node.");
            var newNodeSecondPrefix = new CommentGroupNode("This is the second prefix of the new node.") { Prefix = newNodeFirstPrefix };
            TestRegexNode newNode = new() { Prefix = newNodeSecondPrefix };
            ConcatenationNode target = new(oldNode);

            // Act
            var result = target.ReplaceNode(oldNode, newNode);

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
            TestRegexNode target = new();
            var targetLength = target.ToString().Length;

            // Act
            (var Start, var Length) = target.GetSpan();

            // Assert
            Start.ShouldBe(0);
            Length.ShouldBe(targetLength);
        }

        [TestMethod]
        public void GetSpanShouldReturnTupleWithStartEqualToPrefixLengthAndEqualToToStringLengthMinusPrefixLength()
        {
            // Arrange
            var commentGroup = new CommentGroupNode("X");
            TestRegexNode target = new() { Prefix = commentGroup };
            var prefixLength = commentGroup.ToString().Length;
            var targetLength = target.ToString().Length;

            // Act
            (var Start, var Length) = target.GetSpan();

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

        public override string ToString() => $"{Prefix}RegexTestNode";
    }
}
