﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using Shouldly;

namespace RegexParser.UnitTest.Nodes
{
    [TestClass]
    public class CharacterNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnCharAsString()
        {
            // Arrange
            var target = new CharacterNode('a');

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("a");
        }

        [TestMethod]
        public void CopyingCharacterNodeShouldCopyOriginalCharacter()
        {
            // Arrange
            var target = new CharacterNode('a');

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            CharacterNode characterNode = result.ShouldBeOfType<CharacterNode>();
            characterNode.Character.ShouldBe(target.Character);
        }
    }
}
