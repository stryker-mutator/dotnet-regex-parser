using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;

namespace Stryker.Regex.Parser.UnitTest.Nodes.CharacterClass
{
    [TestClass]
    public class CharacterClassCharacterSetNodeTest
    {
        [TestMethod]
        public void ToStringShouldReturnConcatenationOfChildNodesToString()
        {
            // Arrange
            List<RegexNode> childNodes = [new CharacterNode('a'), new CharacterNode('b'), new CharacterNode('c')];
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
