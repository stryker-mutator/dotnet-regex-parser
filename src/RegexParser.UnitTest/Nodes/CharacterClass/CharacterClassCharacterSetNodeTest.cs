using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.CharacterClass;
using Shouldly;
using System.Collections.Generic;

namespace RegexParser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassCharacterSetNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToString()
        {
            // Arrange
            var childNodes = new List<RegexNode> { new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c') };
            var target = new CharacterClassCharacterSetNode(childNodes);

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("abc");
        }

        [TestMethod]
        public void ToStringOnEmptyNodeShouldReturnEmptyString()
        {
            // Arrange
            var target = new CharacterClassCharacterSetNode();

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe("");
        }
    }
}
