using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.GroupNodes;

namespace Stryker.Regex.Parser.UnitTest.Nodes
{
    [TestClass]
    public class EscapeCharacterNodeTest
    {
        [TestMethod]
        [DataRow('a', '\a')]
        [DataRow('b', '\b')]
        [DataRow('f', '\f')]
        [DataRow('n', '\n')]
        [DataRow('r', '\r')]
        [DataRow('t', '\t')]
        [DataRow('v', '\v')]
        [DataRow('e', (char)27)]
        public void FromCharacterShouldEscapeEscapeCharacters(char escape, char character)
        {
            // Act
            var result = EscapeCharacterNode.FromCharacter(escape);

            // Assert
            result.Escape.ShouldBe(escape.ToString());
            result.Character.ShouldBe(character);
            result.ToString().ShouldBe($"\\{escape}");
        }

        [TestMethod]
        [DataRow('.')]
        [DataRow('$')]
        [DataRow('^')]
        [DataRow('{')]
        [DataRow('[')]
        [DataRow('(')]
        [DataRow('|')]
        [DataRow(')')]
        [DataRow('*')]
        [DataRow('+')]
        [DataRow('?')]
        [DataRow('\\')]
        public void FromCharacterShouldEscapeMetacharacters(char metacharacter)
        {
            // Act
            var result = EscapeCharacterNode.FromCharacter(metacharacter);

            // Assert
            result.Escape.ShouldBe(metacharacter.ToString());
            result.Character.ShouldBe(metacharacter);
            result.ToString().ShouldBe($"\\{metacharacter}");
        }

        [TestMethod]
        [DataRow("00", '\x00')]
        [DataRow("01", '\x01')]
        [DataRow("fe", '\xfe')]
        [DataRow("ff", '\xff')]
        [DataRow("FE", '\xFE')]
        [DataRow("FF", '\xFF')]
        public void FromHexShould2DigitEscapeHexadecimalNumber(string hex, char character)
        {
            // Act
            var result = EscapeCharacterNode.FromHex(hex);

            // Assert
            result.Escape.ShouldBe($"x{hex}");
            result.Character.ShouldBe(character);
            result.ToString().ShouldBe($"\\x{hex}");
        }

        [TestMethod]
        [DataRow("0000", '\u0000')]
        [DataRow("0001", '\u0001')]
        [DataRow("fffe", '\ufffe')]
        [DataRow("ffff", '\uffff')]
        [DataRow("FFFE", '\uFFFE')]
        [DataRow("FFFF", '\uFFFF')]
        public void FromUnicodeShouldEscape4DigitHexadecimalNumber(string hex, char character)
        {
            // Act
            var result = EscapeCharacterNode.FromUnicode(hex);

            // Assert
            result.Escape.ShouldBe($"u{hex}");
            result.Character.ShouldBe(character);
            result.ToString().ShouldBe($"\\u{hex}");
        }

        [TestMethod]
        [DataRow("00", '\x00')]
        [DataRow("01", '\x01')]
        [DataRow("76", '\x3E')]
        [DataRow("77", '\x3F')]
        [DataRow("000", '\x00')]
        [DataRow("001", '\x01')]
        [DataRow("376", '\xFE')]
        [DataRow("377", '\xFF')]
        [DataRow("400", '\x00')]
        [DataRow("776", '\xFE')]
        [DataRow("777", '\xFF')]
        public void FromOctalShouldEscape2Or3DigitOctalNumber(string oct, char character)
        {
            // Act
            var result = EscapeCharacterNode.FromOctal(oct);

            // Assert
            result.Escape.ShouldBe(oct);
            result.Character.ShouldBe(character);
            result.ToString().ShouldBe($"\\{oct}");
        }

        [TestMethod]
        [DataRow('a', '\x01')]
        [DataRow('b', '\x02')]
        [DataRow('y', '\x19')]
        [DataRow('z', '\x1A')]
        [DataRow('A', '\x01')]
        [DataRow('B', '\x02')]
        [DataRow('Y', '\x19')]
        [DataRow('Z', '\x1A')]
        public void FromControlCharacterShouldEscape2Or3DigitOctalNumber(char control, char character)
        {
            // Act
            var result = EscapeCharacterNode.FromControlCharacter(control);

            // Assert
            result.Escape.ShouldBe($"c{control}");
            result.Character.ShouldBe(character);
            result.ToString().ShouldBe($"\\c{control}");
        }

        [TestMethod]
        public void CopyingEscapeNodeShouldCopyOriginalEscape()
        {
            // Arrange
            var target = EscapeCharacterNode.FromCharacter('a');

            // Act
            // RemoveNode returns a copy of the current node.
            var result = target.RemoveNode(new CharacterNode('x'));

            // Assert
            var escapeNode = result.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(target.Escape);
            escapeNode.Character.ShouldBe(target.Character);
        }

        [TestMethod]
        public void ToStringOnEscapeCharacterNodeWithPrefixShouldReturnPrefixBeforeEscapeCharacter()
        {
            // Arrange
            var comment = new CommentGroupNode("This is a comment.");
            var target = EscapeCharacterNode.FromCharacter('a');
            target.Prefix = comment;

            // Act
            var result = target.ToString();

            // Assert
            result.ShouldBe(@"(?#This is a comment.)\a");
        }
    }
}
