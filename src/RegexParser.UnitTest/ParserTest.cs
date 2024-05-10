using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Exceptions;
using RegexParser.Nodes;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.CharacterClass;
using RegexParser.Nodes.GroupNodes;
using RegexParser.Nodes.QuantifierNodes;
using Shouldly;
using System;
using System.Linq;

namespace RegexParser.UnitTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        [DataRow(@"")]
        [DataRow(@"abc")]
        [DataRow(@"a|b|c")]
        [DataRow(@"a1|b2|c3")]
        [DataRow(@"|b2|c3")]
        [DataRow(@"a1||c3")]
        [DataRow(@"a1|b2|")]
        [DataRow(@"\(")]
        [DataRow(@"\[")]
        [DataRow(@"\*")]
        [DataRow(@"\Aabc\Z")]
        [DataRow(@"\p{IsBasicLatin}")]
        [DataRow(@"\P{IsBasicLatin}")]
        [DataRow(@"\p{IsBasicLatin}ab|\P{L}\P{Lu}|cd")]
        [DataRow(@"\x00")]
        [DataRow(@"\xFF")]
        [DataRow(@"\xff")]
        [DataRow(@"\u0000")]
        [DataRow(@"\uFFFF")]
        [DataRow(@"\uffff")]
        [DataRow(@"\cA")]
        [DataRow(@"\cZ")]
        [DataRow(@"\ca")]
        [DataRow(@"\cz")]
        [DataRow(@"(abc)")]
        [DataRow(@"(a)b(c)")]
        [DataRow(@"(a(b)c)")]
        [DataRow(@"(a(b1|b2|b3)c)")]
        [DataRow(@"(?:abc)")]
        [DataRow(@"(?:a)b(?:c)")]
        [DataRow(@"(?:a(?:b)c)")]
        [DataRow(@"(?:a(?:b1|b2|b3)c)")]
        [DataRow(@"(?>abc)")]
        [DataRow(@"(?>a)b(?>c)")]
        [DataRow(@"(?>a(?>b)c)")]
        [DataRow(@"(?>a(?>b1|b2|b3)c)")]
        [DataRow(@"(?<name>abc)")]
        [DataRow(@"(?'first'a)b(?<last>c)")]
        [DataRow(@"(?<outer>a(?'inner'b)c)")]
        [DataRow(@"(?'outer'a(?<inner>b1|b2|b3)c)")]
        [DataRow(@"(?(then)then|else)")]
        [DataRow(@"(?(th(?(innerthen)innerthen|inn(innercap)erelse)en)the(outercap1)n|els(outercap2)e)")]
        [DataRow(@"(?(the(outercap1)n)th(outercap2)en|el(?(innerthen)innerthen|innerelse)se)")]
        [DataRow(@"(?(th(outercap1)en)th(?(innerthen)innerthen|innerelse)en|el(outercap2)se)")]
        [DataRow(@"(?(?(innerthen)innerthen|innerelse)then|else)")]
        [DataRow(@"(ab){1,2}|cd")]
        [DataRow(@"(ab){1,2|cd")]
        [DataRow(@"(ab){|cd")]
        [DataRow(@"(ab){")]
        [DataRow(@"(ab){1,2}?|cd")]
        [DataRow(@"(ab){1,2?|cd")]
        [DataRow(@"(ab){?|cd")]
        [DataRow(@"(ab){?")]
        [DataRow(@"(?#This is a comment.)")]
        [DataRow(@"(?#This is a comment.)a")]
        [DataRow(@"a(?#This is a comment.)")]
        [DataRow(@"((?#This is a comment.))")]
        [DataRow(@"((?#This is a comment.)a)")]
        [DataRow(@"(a(?#This is a comment.))")]
        [DataRow(@"(?#This is a comment.)|b")]
        [DataRow(@"(?#This is a comment.)a|b")]
        [DataRow(@"a(?#This is a comment.)|b")]
        [DataRow(@"a|(?#This is a comment.)")]
        [DataRow(@"a|b(?#This is a comment.)")]
        [DataRow(@"(?#This is the first comment.)(?#This is the second comment.)(?#This is the third comment.)a")]
        public void ParseShouldReturnRegexNodeWithOriginalRegexPattern(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ToString().ShouldBe(pattern);
        }

        [TestMethod]
        public void ConstructorWithInvalidRegexThrowsRegexParseException()
        {
            // Act
            Action act = () => new Parser(")");

            // Assert
            act.ShouldThrow<RegexParseException>();
        }

        [TestMethod]
        public void ParseEmptyStringReturnsEmptyNode()
        {
            // Arrange
            var target = new Parser("");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void CharacterNodesAreAddedToConcatenationNode()
        {
            // Arrange
            var target = new Parser("abc");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ShouldBeOfType<ConcatenationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            root.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            root.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        [DataRow("a|b|c")]
        [DataRow("a1|b2|c3")]
        public void ConcatenationNodesAreAddedToAlternationNode(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            root.ChildNodes.ElementAt(1).ShouldBeOfType<ConcatenationNode>();
            root.ChildNodes.ElementAt(2).ShouldBeOfType<ConcatenationNode>();
        }

        [TestMethod]
        public void EmptyFirstAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("|b|c");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root. ShouldBeOfType<AlternationNode>();
            root. ChildNodes.Count().ShouldBe(3);
            root. ChildNodes.First().ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void EmptyMiddleAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("a||c");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            root.ChildNodes.ElementAt(1).ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void EmptyLastAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("a|b|");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            root.ChildNodes.ElementAt(2).ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        [DataRow(".")]
        [DataRow("$")]
        [DataRow("^")]
        [DataRow("{")]
        [DataRow("[")]
        [DataRow("(")]
        [DataRow("|")]
        [DataRow(")")]
        [DataRow("*")]
        [DataRow("+")]
        [DataRow("?")]
        [DataRow("\\")]
        public void ParsingBackslashMetaCharacterShouldRetunEscapCharacterWithEscapedMetaCharacter(string metaCharacter)
        {
            // Arrange
            var target = new Parser($@"\{metaCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(metaCharacter);
        }

        [TestMethod]
        public void ParsingBackslashUppercaseAShouldReturnStartOfStringNode()
        {
            // Arrange
            var target = new Parser(@"\A");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<StartOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseZShouldReturnEndOfStringZNode()
        {
            // Arrange
            var target = new Parser(@"\Z");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfStringZNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseZShouldReturnEndOfStringNode()
        {
            // Arrange
            var target = new Parser(@"\z");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseBShouldReturnWordBoundaryNode()
        {
            // Arrange
            var target = new Parser(@"\b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<WordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseBShouldReturnNonWordBoundaryNode()
        {
            // Arrange
            var target = new Parser(@"\B");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<NonWordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseGShouldReturnContiguousMatchNode()
        {
            // Arrange
            var target = new Parser(@"\G");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<ContiguousMatchNode>();
        }

        [TestMethod]
        public void ParsingCaretShouldReturnStartOfLineNode()
        {
            // Arrange
            var target = new Parser("^");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<StartOfLineNode>();
        }

        [TestMethod]
        public void ParsingDollarShouldReturnEndOfLineNode()
        {
            // Arrange
            var target = new Parser("$");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfLineNode>();
        }

        [TestMethod]
        public void ParsingDotShouldReturnAnyCharacterNode()
        {
            // Arrange
            var target = new Parser(".");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<AnyCharacterNode>();
        }

        [TestMethod]
        [DataRow('w')]
        [DataRow('W')]
        [DataRow('s')]
        [DataRow('S')]
        [DataRow('d')]
        [DataRow('D')]
        public void ParsingBackslashShorthandCharacterShouldReturnShorthandWithChar(char shorthandCharacter)
        {
            // Arrange
            var target = new Parser($@"\{shorthandCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassShorthandNode characterClassShorthandNode = childNode.ShouldBeOfType<CharacterClassShorthandNode>();
            characterClassShorthandNode.Shorthand.ShouldBe(shorthandCharacter);
        }

        [TestMethod]
        public void ParsingBackslashLowercasePUnicodeCategoryShouldReturnUnicodeCategoryNodeWithRightCategory()
        {
            // Arrange
            var category = "IsBasicLatin";
            var target = new Parser($@"\p{{{category}}}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            UnicodeCategoryNode unicodeCategoryNode = childNode.ShouldBeOfType<UnicodeCategoryNode>();
            unicodeCategoryNode.Category.ShouldBe(category);
            unicodeCategoryNode.Negated.ShouldBe(false);
        }

        [TestMethod]
        public void ParsingBackslashUppercasePUnicodeCategoryShouldReturnNegatedUnicodeCategoryNodeWithRightCategory()
        {
            // Arrange
            var category = "IsBasicLatin";
            var target = new Parser($@"\P{{{category}}}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            UnicodeCategoryNode unicodeCategoryNode = childNode.ShouldBeOfType<UnicodeCategoryNode>();
            unicodeCategoryNode.Category.ShouldBe(category);
            unicodeCategoryNode.Negated.ShouldBe(true);
        }

        [TestMethod]
        [DataRow("a")]
        [DataRow("e")]
        [DataRow("f")]
        [DataRow("n")]
        [DataRow("r")]
        [DataRow("t")]
        [DataRow("v")]
        public void ParsingBackslashEscapeCharacterShouldReturnEscapecharacterNodeWithEscapeCharacter(string escape)
        {
            // Arrange
            var target = new Parser($@"\{escape}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(escape);
        }

        [TestMethod]
        [DataRow("x00")]
        [DataRow("x01")]
        [DataRow("xFE")]
        [DataRow("xFF")]
        [DataRow("xfe")]
        [DataRow("xff")]
        public void ParsingBackslashLowercaseXHexHexShouldReturnEscapecharacterNodeWithEscapeCharacter(string hexCharacter)
        {
            // Arrange
            var target = new Parser($@"\{hexCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(hexCharacter);
        }

        [TestMethod]
        [DataRow("u0000")]
        [DataRow("u0001")]
        [DataRow("uFFFE")]
        [DataRow("uFFFF")]
        [DataRow("ufffe")]
        [DataRow("uffff")]
        public void ParsingBackslashLowercaseUHexHexHexHexShouldReturnEscapecharacterNodeWithEscapeCharacter(string unicodeCharacter)
        {
            // Arrange
            var target = new Parser($@"\{unicodeCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(unicodeCharacter);
        }

        [TestMethod]
        [DataRow("cA")]
        [DataRow("cB")]
        [DataRow("cX")]
        [DataRow("cZ")]
        [DataRow("ca")]
        [DataRow("cb")]
        [DataRow("cx")]
        [DataRow("cz")]
        public void ParsingBackslashLowercaseCAlphaShouldReturnEscapecharacterNodeWithEscapeCharacter(string controlCharacter)
        {
            // Arrange
            var target = new Parser($@"\{controlCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(controlCharacter);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(9)]
        public void ParsingBackslashDigitShouldReturnBackreferenceNodeWithGroupNumber(int groupNumber)
        {
            // Arrange
            var target = new Parser($@"()()()()()()()()()\{groupNumber}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            BackreferenceNode backreferenceNode = root.ChildNodes.Last().ShouldBeOfType<BackreferenceNode>();
            backreferenceNode.GroupNumber.ShouldBe(groupNumber);
        }

        [TestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashLowercaseKNameBetweenAngledBracketsShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsFalseAndUseKIsTrue(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\k<{name}>");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            NamedReferenceNode namedReferenceNode = root.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(false);
            namedReferenceNode.UseK.ShouldBe(true);
        }

        [TestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashLowercaseKNameBetweenSingleQuotesShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsTrueAndUseKIsTrue(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\k'{name}'");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            NamedReferenceNode namedReferenceNode = root.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(true);
            namedReferenceNode.UseK.ShouldBe(true);
        }

        [TestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashNameBetweenAngledBracketsShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsFalseAndUseKIsFalse(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\<{name}>");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            NamedReferenceNode namedReferenceNode = root.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(false);
            namedReferenceNode.UseK.ShouldBe(false);
        }

        [TestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashNameBetweenSingleQuotesShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsTrueAndUseKIsFalse(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\'{name}'");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            NamedReferenceNode namedReferenceNode = root.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(true);
            namedReferenceNode.UseK.ShouldBe(false);
        }

        [TestMethod]
        public void ParsingEmptyParenthesesShouldReturnCaptureGroupWithEmptyNode()
        {
            // Arrange
            var target = new Parser("()");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            CaptureGroupNode captureGroupNode = childNode.ShouldBeOfType<CaptureGroupNode>();
            var groupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingParenthesesWithCharactersShouldReturnCaptureGroupWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<CaptureGroupNode>();
            var captureGroupchildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = captureGroupchildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingParenthesesWithAlternationShouldReturnCaptureGroupWithAlternationNode()
        {
            // Arrange
            var target = new Parser("(a|b|c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<CaptureGroupNode>();
            var captureGroupchildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            AlternationNode alternationNode = captureGroupchildNode.ShouldBeOfType<AlternationNode>();
            alternationNode.ChildNodes.Count().ShouldBe(3);
            ConcatenationNode alternate = alternationNode.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            var alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            alternateChildNode.ShouldBeOfType<CharacterNode>();
            alternate = alternationNode.ChildNodes.ElementAt(1).ShouldBeOfType<ConcatenationNode>();
            alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            alternateChildNode.ShouldBeOfType<CharacterNode>();
            alternate = alternationNode.ChildNodes.ElementAt(2).ShouldBeOfType<ConcatenationNode>();
            alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            alternateChildNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingMultipleParenthesesShouldReturnMultipleCaptureGroupNodes()
        {
            // Arrange
            var target = new Parser("(a)b(c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(3);
            CaptureGroupNode captureGroupNode = root.ChildNodes.First().ShouldBeOfType<CaptureGroupNode>();
            var captureGroupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = captureGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            var concatentationChildNode = concatenationNode.ChildNodes.ShouldHaveSingleItem();
            concatentationChildNode.ShouldBeOfType<CharacterNode>();

            root.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();

            captureGroupNode = root.ChildNodes.Last().ShouldBeOfType<CaptureGroupNode>();
            captureGroupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            concatenationNode = captureGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatentationChildNode = concatenationNode.ChildNodes.ShouldHaveSingleItem();
            concatentationChildNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNestedParenthesesShouldReturnNestedCaptureGroupNodes()
        {
            // Arrange
            var target = new Parser("(a(b)c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            CaptureGroupNode captureGroupNode = childNode.ShouldBeOfType<CaptureGroupNode>();
            var captureGroupChild = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            captureGroupChild.ShouldBeOfType<ConcatenationNode>();
            captureGroupChild.ChildNodes.Count().ShouldBe(3);
            captureGroupChild.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            captureGroupChild.ChildNodes.Last().ShouldBeOfType<CharacterNode>();

            CaptureGroupNode nestedGroup = captureGroupChild.ChildNodes.ElementAt(1).ShouldBeOfType<CaptureGroupNode>();
            var nestedGroupChildNode = nestedGroup.ChildNodes.ShouldHaveSingleItem();
            nestedGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            var nestedGroupCharacterNode = nestedGroupChildNode.ChildNodes.ShouldHaveSingleItem();
            nestedGroupCharacterNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithNameBetweenAngledBracketsShouldReturnNamedGroupNodeWithUseQuotesIsFalse()
        {
            // Arrange
            var target = new Parser("(?<name>)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            NamedGroupNode namedGroupNode = childNode.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe("name");
            namedGroupNode.UseQuotes.ShouldBeFalse();
            var groupChildNode = namedGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithNameBetweenSingleQuotesShouldReturnNamedGroupNodeWithUseQuotesIsTrue()
        {
            // Arrange
            var target = new Parser("(?'name')");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            NamedGroupNode namedGroupNode = childNode.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe("name");
            namedGroupNode.UseQuotes.ShouldBeTrue();
            var groupChildNode = namedGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithCharactersShouldReturnNamedGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?<name>abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<NamedGroupNode>();
            var groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNonCaptureGroupShouldReturNonCaptureGroupNode()
        {
            // Arrange
            var target = new Parser("(?:)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<NonCaptureGroupNode>();
            var groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNonCaptureGroupWithCharactersShouldReturnNonCaptureGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?:abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<NonCaptureGroupNode>();
            var groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingAtomicGroupShouldReturnAtomicGroupNode()
        {
            // Arrange
            var target = new Parser("(?>)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<AtomicGroupNode>();
            var groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingAtomicGroupWithCharactersShouldReturnAtomicGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?>abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<AtomicGroupNode>();
            var groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookaheadGroupShouldReturnLookaroundGroupNodeWithLookaheadIsTrueAndPossitiveIsTrue()
        {
            // Arrange
            var target = new Parser("(?=)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookaheadGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?=abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookaheadGroupShouldReturnLookaroundGroupNodeWithLookaheadIsTrueAndPossitiveIsFalse()
        {
            // Arrange
            var target = new Parser("(?!)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookaheadGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?!abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookbehindGroupShouldReturnLookaroundGroupNodeWithLookaheadIsFalseAndPossitiveIsTrue()
        {
            // Arrange
            var target = new Parser("(?<=)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookbehindGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?<=abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookbehindGroupShouldReturnLookaroundGroupNodeWithLookaheadIsFalseAndPossitiveIsFalse()
        {
            // Arrange
            var target = new Parser("(?<!)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookbehindGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            var target = new Parser("(?<!abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            var groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingConditionalGroupShouldReturnConditionalGroupNodeWithGroupNodeAsFirstChildAndAlternationWithTwoAlternatesAsSecondChild()
        {
            // Arrange
            var target = new Parser("(?(condition)then|else)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            ConditionalGroupNode conditionalGroupNode = childNode.ShouldBeOfType<ConditionalGroupNode>();
            conditionalGroupNode.ChildNodes.Count().ShouldBe(2);

            conditionalGroupNode.ChildNodes.First().ShouldBeOfType<CaptureGroupNode>();
            var condition = conditionalGroupNode.ChildNodes.First().ChildNodes.ShouldHaveSingleItem();
            condition.ToString().ShouldBe("condition");

            var alternation = conditionalGroupNode.ChildNodes.Last().ShouldBeOfType<AlternationNode>();
            alternation.ChildNodes.Count().ShouldBe(2);
            alternation.ChildNodes.First().ToString().ShouldBe("then");
            alternation.ChildNodes.Last().ToString().ShouldBe("else");
        }

        [TestMethod]
        public void ParsingModeModifierGroupWithoutSubexpressionShouldReturnModeModifierGroupNodeWithModesAndEmotyNodeAsChildNode()
        {
            // Arrange
            var target = new Parser("(?imnsx-imnsx+imnsx)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imnsx-imnsx+imnsx");
            var empty = modeModifierGroupNode.ChildNodes.ShouldHaveSingleItem();
            empty.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingModeModifierGroupWithSubexpressionAfterColonShouldReturnModeModifierGroupNodeWithModesAndChildNode()
        {
            // Arrange
            var target = new Parser("(?imnsx-imnsx+imnsx:abc)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imnsx-imnsx+imnsx");
            var modeModifierChildNode = modeModifierGroupNode.ChildNodes.ShouldHaveSingleItem();
            modeModifierChildNode.ShouldBeOfType<ConcatenationNode>();
            modeModifierChildNode.ChildNodes.Count().ShouldBe(3);
            modeModifierChildNode.ToString().ShouldBe("abc");
        }

        [TestMethod]
        public void UsingSameModeModifierMultipleTimesShouldBeAllowed()
        {
            // Arrange
            var target = new Parser("(?iimmnnssxxxx-iimmnnssxxxx+iimmnnssxxxx)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("iimmnnssxxxx-iimmnnssxxxx+iimmnnssxxxx");
        }

        [TestMethod]
        public void UsingMinusAndPlusMultipleTimesInModeModfierGroupShouldBeAllowed()
        {
            // Arrange
            var target = new Parser("(?imn-s+x-im+-+ns-x+im)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imn-s+x-im+-+ns-x+im");
        }

        [TestMethod]
        public void ParsingBalancingGroupWithOneNameInAngledBracketsShouldReturnBalancingGroupWithBalancedGroupNameAndUseQuotesFalse()
        {
            // Arrange
            var target = new Parser("(?<balancedGroup>)(?<-balancedGroup>)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            BalancingGroupNode balancingGroupNode = root.ChildNodes.ElementAt(1).ShouldBeOfType<BalancingGroupNode>();
            balancingGroupNode.BalancedGroupName.ShouldBe("balancedGroup");
            balancingGroupNode.Name.ShouldBeNull();
            balancingGroupNode.UseQuotes.ShouldBeFalse();
        }

        [TestMethod]
        public void ParsingBalancingGroupWithTwoNamesInAngledBracketsShouldReturnBalancingGroupWithNameAndBalancedGroupNameAndUseQuotesFalse()
        {
            // Arrange
            var target = new Parser("(?<balancedGroup>)(?<name-balancedGroup>)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            BalancingGroupNode balancingGroupNode = root.ChildNodes.ElementAt(1).ShouldBeOfType<BalancingGroupNode>();
            balancingGroupNode.BalancedGroupName.ShouldBe("balancedGroup");
            balancingGroupNode.Name.ShouldBe("name");
            balancingGroupNode.UseQuotes.ShouldBeFalse();
        }

        [TestMethod]
        public void ParsingBalancingGroupWithOneNameInSingleQuotesShouldReturnBalancingGroupWithBalancedGroupNameAndUseQuotesTrue()
        {
            // Arrange
            var target = new Parser("(?<balancedGroup>)(?'-balancedGroup')");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            BalancingGroupNode balancingGroupNode = root.ChildNodes.ElementAt(1).ShouldBeOfType<BalancingGroupNode>();
            balancingGroupNode.BalancedGroupName.ShouldBe("balancedGroup");
            balancingGroupNode.Name.ShouldBeNull();
            balancingGroupNode.UseQuotes.ShouldBeTrue();
        }

        [TestMethod]
        public void ParsingBalancingGroupWithTwoNamesInSingleQuotesShouldReturnBalancingGroupWithNameAndBalancedGroupNameAndUseQuotesTrue()
        {
            // Arrange
            var target = new Parser("(?<balancedGroup>)(?'name-balancedGroup')");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            BalancingGroupNode balancingGroupNode = root.ChildNodes.ElementAt(1).ShouldBeOfType<BalancingGroupNode>();
            balancingGroupNode.BalancedGroupName.ShouldBe("balancedGroup");
            balancingGroupNode.Name.ShouldBe("name");
            balancingGroupNode.UseQuotes.ShouldBeTrue();
        }

        [TestMethod]
        public void ParsingQuestionMarkShouldReturnQuantifierQuestionMarkNode()
        {
            // Arrange
            var target = new Parser("a?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void QuantifierQuestionMarkCanBeLazy()
        {
            // Arrange
            var target = new Parser("a??");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void ParsingPlusShouldReturnQuantifierPlusNode()
        {
            // Arrange
            var target = new Parser("a+");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<QuantifierPlusNode>();
        }

        [TestMethod]
        public void QuantifierPlusCanBeLazy()
        {
            // Arrange
            var target = new Parser("a+?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierPlusNode>();
        }

        [TestMethod]
        public void ParsingStarShouldReturnQuantifierStarNode()
        {
            // Arrange
            var target = new Parser("a*");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<QuantifierStarNode>();
        }

        [TestMethod]
        public void QuantifierStarCanBeLazy()
        {
            // Arrange
            var target = new Parser("a*?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierStarNode>();
        }

        [TestMethod]
        public void ParsingIntegerBetweenCurlyBracketsShouldReturnQuantifierNNodeWithIntegerAsN()
        {
            // Arrange
            var target = new Parser("a{5}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNNode>();
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void QuantifierNCanBeLazy()
        {
            // Arrange
            var target = new Parser("a{5}?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierNNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesBetweenCurlyBracketsShouldReturnQuantifierNNodeWithLeadingZeroes()
        {
            // Arrange
            var target = new Parser("a{05}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNNode>();
            quantifierNode.OriginalN.ShouldBe("05");
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void ParsingIntegerCommaBetweenCurlyBracketsShouldReturnQuantifierNNodeWithIntegerAsN()
        {
            // Arrange
            var target = new Parser("a{5,}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNOrMoreNode>();
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void QuantifierNOrMoreCanBeLazy()
        {
            // Arrange
            var target = new Parser("a{5,}?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierNOrMoreNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesCommaBetweenCurlyBracketsShouldReturnQuantifierNNodeWithLeadingZeroes()
        {
            // Arrange
            var target = new Parser("a{05,}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNOrMoreNode>();
            quantifierNode.OriginalN.ShouldBe("05");
        }

        [TestMethod]
        public void ParsingIntegerCommaIntegerBetweenCurlyBracketsShouldReturnQuantifierNMNodeIfNIsLessThanM()
        {
            // Arrange
            var target = new Parser("a{9,10}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
            quantifierNode.N.ShouldBe(9);
            quantifierNode.M.ShouldBe(10);
        }

        [TestMethod]
        public void ParsingIntegerNCommaIntegerMBetweenCurlyBracketsShouldReturnQuantifierNMNodeIfNIsEqualToM()
        {
            // Arrange
            var target = new Parser("a{10,10}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
            quantifierNode.N.ShouldBe(10);
            quantifierNode.M.ShouldBe(10);
        }

        [TestMethod]
        public void ParsingIntegerNCommaIntegerMBetweenCurlyBracketsShouldThrowRegexParseExceptionIfNIsGreaterThanM()
        {
            // Act
            Action act = () => {
                var target = new Parser("a{11,10}");
                target.Parse();
            };

            // Assert
            act.ShouldThrow<RegexParseException>();

        }

        [TestMethod]
        public void QuantifierNMCanBeLazy()
        {
            // Arrange
            var target = new Parser("a{5,10}?");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var lazyNode = childNode.ShouldBeOfType<LazyNode>();
            var quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            quantifierNode.ShouldBeOfType<QuantifierNMNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesCommaIntegerWithLeadingZeroesBetweenCurlyBracketsShouldReturnQuantifierNNodeWithWithLeadingZeroes()
        {
            // Arrange
            var target = new Parser("a{05,010}");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
            quantifierNode.OriginalN.ShouldBe("05");
            quantifierNode.OriginalM.ShouldBe("010");
        }

        [TestMethod]
        [DataRow("{")]
        [DataRow("{a")]
        [DataRow("{1")]
        [DataRow("{1a")]
        [DataRow("{1,")]
        [DataRow("{1,a")]
        [DataRow("{1,2")]
        [DataRow("{1,2a")]
        public void ParsingOpeningCurlyBracketNotFollowingQuantifierFormatShourdReturnBracketAsCharacterNode(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            var characterNode = root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("{");
        }

        [TestMethod]
        [DataRow("a?")]
        [DataRow("^?")]
        [DataRow("$?")]
        [DataRow(".?")]
        [DataRow("[a]?")]
        [DataRow("()?")]
        [DataRow(@"()\1?")]
        [DataRow(@"\d?")]
        [DataRow(@"\p{Lu}?")]
        [DataRow(@"\{?")]
        [DataRow(@"(?<name>)\k<name>?")]
        public void QuantifierAfterNonAlternationOrQuantifierNodeShouldBeValid(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            root.ChildNodes.Last().ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        [DataRow("a?a?")]
        [DataRow("a?^?")]
        [DataRow("a?$?")]
        [DataRow("a?.?")]
        [DataRow("a?[a]?")]
        [DataRow("a?()?")]
        [DataRow(@"a?()\1?")]
        [DataRow(@"a?\d?")]
        [DataRow(@"a?\p{Lu}?")]
        [DataRow(@"a?\{?")]
        [DataRow(@"a?(?<name>)\k<name>?")]
        public void QuantifierAfterNonAlternationOrQuantifierAfterQuantifierNodeShouldBeValid(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            root.ChildNodes.Last().ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void ParsingCharactersBetweenSquareBracketsNotStartingWithCaretShouldReturnCharacterClassWithNegationFalseAndCharactersInCharacterSet()
        {
            // Arrange
            var target = new Parser("[abc]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeFalse();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            var characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void ParsingCharactersBetweenSquareBracketsStartingWithCaretShouldReturnCharacterClassWithNegationTrue()
        {
            // Arrange
            var target = new Parser("[^abc]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeTrue();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            var characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void DashAtStartOfCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            var target = new Parser("[-abc]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            var characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterStartingCaretInCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            var target = new Parser("[^-abc]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeTrue();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            var characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAtEndOfCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            var target = new Parser("[abc-]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            var characterNode = characterSet.ChildNodes.Last().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashBeforeCharacterClassInsideCharacterClassShouldBeParsedAsCharacterClassSubtraction()
        {
            // Arrange
            var target = new Parser("[abc-[a]]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.ChildNodes.Count().ShouldBe(2);

            var characterSet = characterClassNode.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);

            var subtraction = characterClassNode.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
            var subtractionCharacterSet = subtraction.ChildNodes.ShouldHaveSingleItem();
            var subtractedCharacter = subtractionCharacterSet.ChildNodes.ShouldHaveSingleItem();
            subtractedCharacter.ShouldBeOfType<CharacterNode>();
            subtractedCharacter.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void CharacterClassSubtractionCanBeNested()
        {
            // Arrange
            var target = new Parser("[abc-[ab-[c]]]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var childNode = root.ChildNodes.ShouldHaveSingleItem();
            var characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.ChildNodes.Count().ShouldBe(2);

            var characterSet = characterClassNode.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);

            var subtraction = characterClassNode.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
            subtraction.ChildNodes.Count().ShouldBe(2);

            var subtractionCharacterSet = subtraction.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            subtractionCharacterSet.ChildNodes.Count().ShouldBe(2);


            var nestedSubtraction = subtraction.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
            nestedSubtraction.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>().ToString().ShouldBe("c");
        }

        [TestMethod]
        [DataRow('w')]
        [DataRow('W')]
        [DataRow('s')]
        [DataRow('S')]
        [DataRow('d')]
        [DataRow('D')]
        public void CharacterClassShorthandIsValidInCharacterClass(char shorthand)
        {
            // Arrange
            var target = new Parser($@"[\{shorthand}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassShorthandNode>();
            shorthandNode.Shorthand.ShouldBe(shorthand);
        }

        [TestMethod]
        public void UnicodeCategoryIsValidInCharacterClass()
        {
            // Arrange
            var category = "IsBasicLatin";
            var target = new Parser($@"[\p{{{category}}}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<UnicodeCategoryNode>();
            shorthandNode.Category.ShouldBe(category);
            shorthandNode.Negated.ShouldBeFalse();
        }

        [TestMethod]
        public void NegatedUnicodeCategoryIsValidInCharacterClass()
        {
            // Arrange
            var category = "IsBasicLatin";
            var target = new Parser($@"[\P{{{category}}}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<UnicodeCategoryNode>();
            shorthandNode.Category.ShouldBe(category);
            shorthandNode.Negated.ShouldBeTrue();
        }

        [TestMethod]
        [DataRow("a")]
        [DataRow("b")]
        [DataRow("e")]
        [DataRow("f")]
        [DataRow("n")]
        [DataRow("r")]
        [DataRow("t")]
        [DataRow("v")]
        public void EscapeCharacterIsValidInCharacterClass(string escapedChar)
        {
            // Arrange
            var target = new Parser($@"[\{escapedChar}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(escapedChar);
        }

        [TestMethod]
        [DataRow(".")]
        [DataRow("$")]
        [DataRow("^")]
        [DataRow("{")]
        [DataRow("[")]
        [DataRow("(")]
        [DataRow("|")]
        [DataRow(")")]
        [DataRow("*")]
        [DataRow("+")]
        [DataRow("?")]
        [DataRow("\\")]
        public void EscapedMetacharacterIsValidInCharacterClass(string escapedChar)
        {
            // Arrange
            var target = new Parser($@"[\{escapedChar}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(escapedChar);
        }

        [TestMethod]
        [DataRow("x00")]
        [DataRow("x01")]
        [DataRow("xFE")]
        [DataRow("xFF")]
        [DataRow("xfe")]
        [DataRow("xff")]
        public void HexadecimalEscapeIsValidInCharacterClass(string hex)
        {
            // Arrange
            var target = new Parser($@"[\{hex}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(hex);
        }

        [TestMethod]
        [DataRow("u0000")]
        [DataRow("u0001")]
        [DataRow("uFFFE")]
        [DataRow("uFFFF")]
        [DataRow("ufffe")]
        [DataRow("uffff")]
        public void UnicodeEscapeIsValidInCharacterClass(string hex)
        {
            // Arrange
            var target = new Parser($@"[\{hex}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(hex);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("1")]
        [DataRow("6")]
        [DataRow("7")]
        [DataRow("00")]
        [DataRow("01")]
        [DataRow("76")]
        [DataRow("77")]
        [DataRow("000")]
        [DataRow("001")]
        [DataRow("776")]
        [DataRow("777")]
        public void OctalEscapeIsValidInCharacterClass(string oct)
        {
            // Arrange
            var target = new Parser($@"[\{oct}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(oct);
        }

        [TestMethod]
        [DataRow("cA")]
        [DataRow("cB")]
        [DataRow("cX")]
        [DataRow("cZ")]
        [DataRow("ca")]
        [DataRow("cb")]
        [DataRow("cx")]
        [DataRow("cz")]
        public void ControlCharacterIsValidInCharacterClass(string oct)
        {
            // Arrange
            var target = new Parser($@"[\{oct}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(oct);
        }

        [TestMethod]
        public void EscapeCharacterIsValidInTheMiddleOfACharacterClass()
        {
            // Arrange
            var target = new Parser($@"[a\nb]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            var escapeChar = characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe("n");
        }

        [TestMethod]
        public void EscapeCharacterIsValidAtTheEndOfACharacterClass()
        {
            // Arrange
            var target = new Parser($@"[a\n]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(2);
            var escapeChar = characterSet.ChildNodes.Last().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe("n");
        }

        [TestMethod]
        [DataRow("a", "z")]
        [DataRow("a", "a")]
        public void DashBetweenCharactersInCharacterClassShouldBeRange(string start, string end)
        {
            // Arrange
            var target = new Parser($"[{start}-{end}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var range = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassRangeNode>();
            range.ChildNodes.Count().ShouldBe(2);
            range.ChildNodes.First().ShouldBeOfType<CharacterNode>().ToString().ShouldBe(start);
            range.ChildNodes.Last().ShouldBeOfType<CharacterNode>().ToString().ShouldBe(end);
        }

        [TestMethod]
        public void DashAfterCharacterClassShorthandInCharacterClassShouldBeLiteral()
        {
            // Arrange
            var target = new Parser(@"[\d-z]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterUnicodeCategoryInCharacterClassShouldBeLiteral()
        {
            // Arrange
            var target = new Parser(@"[\p{IsBasicLatin}-z]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterRangeInCharacterClassShouldBeLiteral()
        {
            // Arrange
            var target = new Parser(@"[a-z-z]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        [DataRow("{", "}")]
        [DataRow("{", "{")]
        public void DashBetweenEscapeCharactersInCharacterClassShouldBeRange(string start, string end)
        {
            // Arrange
            var target = new Parser($@"[\{start}-\{end}]");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            var characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            var range = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassRangeNode>();
            range.ChildNodes.Count().ShouldBe(2);
            range.ChildNodes.First().ShouldBeOfType<EscapeCharacterNode>().Escape.ShouldBe(start);
            range.ChildNodes.Last().ShouldBeOfType<EscapeCharacterNode>().Escape.ShouldBe(end);
        }

        [TestMethod]
        public void ParsingCommentGroupShouldAddCommentAsPrefixForNextToken()
        {
            // Arrange
            var target = new Parser("(?#This is a comment.)a");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            var comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfPatternShouldAddCommentAsPrefixForEmptyNode()
        {
            // Arrange
            var target = new Parser("a(?#This is a comment.)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            var emptyNode = root.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            var comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingPatternWithOnlyCommentGroupShouldReturnOnlyEmptyNodeWithPrefix()
        {
            // Arrange
            var target = new Parser("(?#This is a comment.)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root.ShouldBeOfType<EmptyNode>();
            var comment = root.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupInGroupShouldAddCommentAsPrefixForNextTokenInGroup()
        {
            // Arrange
            var target = new Parser("((?#This is a comment.)a)");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            var groupConcat = group.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<ConcatenationNode>();
            var characterNode = groupConcat.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            var comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfAGroupShouldAddCommentAsPrefixForEmptyNodeAtTheEndOfTheGroup()
        {
            // Arrange
            var target = new Parser("(a(?#This is a comment.))");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            var groupConcatenation = group.ChildNodes.ShouldHaveSingleItem();
            groupConcatenation.ChildNodes.Count().ShouldBe(2);
            var emptyNode = groupConcatenation.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            var comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingGroupWithOnlyCommentGroupShouldAddCommentAsPrefixForEmptyNodeAsOnlyChildOfTheGroup()
        {
            // Arrange
            var target = new Parser("((?#This is a comment.))");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            var emptyNode = group.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EmptyNode>();
            var comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupInAlternationShouldAddCommentAsPrefixForNextToken()
        {
            // Arrange
            var target = new Parser("(?#This is a comment.)a|b");

            // Act
            var result = target.Parse();

            // Assert
            var root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            var alternate = root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            var characterNode = alternate.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            var comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfAlternateShouldAddCommentAsPrefixForEmptyNode()
        {
            // Arrange
            var target = new Parser("a(?#This is a comment.)|b");

            // Act
            var result = target.Parse();

            // Assert
            var root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            var alternate = root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            alternate.ChildNodes.Count().ShouldBe(2);
            var emptyNode = alternate.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            var comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingAlternateWithOnlyCommentGroupShouldAddOnlyEmptyNodeWithPrefixAsAlternate()
        {
            // Arrange
            var target = new Parser("(?#This is a comment.)|b");

            // Act
            var result = target.Parse();

            // Assert
            var root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            var alternate = root.ChildNodes.First().ShouldBeOfType<EmptyNode>();
            var comment = alternate.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingAlternationWithOnlyCommentGroupInLastAlternateShouldAddOnlyEmptyNodeWithPrefixAsAlternate()
        {
            // Arrange
            var target = new Parser("a|(?#This is a comment.)");

            // Act
            var result = target.Parse();

            // Assert
            var root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            var alternate = root.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            var comment = alternate.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingMultipleCommentGroupsOnMultipleTokensShouldAddCommentsAsPrefixesForNextTokens()
        {
            // Arrange
            var target = new Parser("(?#This is the first comment.)a(?#This is the second comment.)b");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            var firstCharacter = root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            var firstComment = firstCharacter.Prefix.ShouldBeOfType<CommentGroupNode>();
            firstComment.Comment.ShouldBe("This is the first comment.");
            var lastCharacter = root.ChildNodes.Last().ShouldBeOfType<CharacterNode>();
            var lastComment = lastCharacter.Prefix.ShouldBeOfType<CommentGroupNode>();
            lastComment.Comment.ShouldBe("This is the second comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupBeforeCommentGroupShouldAddCommentAsPrefixForNextCommentGroup()
        {
            // Arrange
            var target = new Parser("(?#This is the first comment.)(?#This is the second comment.)(?#This is the third comment.)a");

            // Act
            var result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            var characterNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            var comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is the third comment.");
            var secondComment = comment.Prefix.ShouldBeOfType<CommentGroupNode>();
            secondComment.Comment.ShouldBe("This is the second comment.");
            var lastComment = secondComment.Prefix.ShouldBeOfType<CommentGroupNode>();
            lastComment.Comment.ShouldBe("This is the first comment.");
        }
    }
}
