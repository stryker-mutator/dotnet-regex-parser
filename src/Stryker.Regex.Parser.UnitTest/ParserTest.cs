using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Regex.Parser.Exceptions;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.AnchorNodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;

namespace Stryker.Regex.Parser.UnitTest
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
            Parser target = new(pattern);

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
            _ = act.ShouldThrow<RegexParseException>();
        }

        [TestMethod]
        public void ParseEmptyStringReturnsEmptyNode()
        {
            // Arrange
            Parser target = new("");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void CharacterNodesAreAddedToConcatenationNode()
        {
            // Arrange
            Parser target = new("abc");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<ConcatenationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            _ = root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = root.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = root.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        [DataRow("a|b|c")]
        [DataRow("a1|b2|c3")]
        public void ConcatenationNodesAreAddedToAlternationNode(string pattern)
        {
            // Arrange
            Parser target = new(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            _ = root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            _ = root.ChildNodes.ElementAt(1).ShouldBeOfType<ConcatenationNode>();
            _ = root.ChildNodes.ElementAt(2).ShouldBeOfType<ConcatenationNode>();
        }

        [TestMethod]
        public void EmptyFirstAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            Parser target = new("|b|c");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            _ = root.ChildNodes.First().ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void EmptyMiddleAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            Parser target = new("a||c");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            _ = root.ChildNodes.ElementAt(1).ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void EmptyLastAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            Parser target = new("a|b|");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            _ = root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(3);
            _ = root.ChildNodes.ElementAt(2).ShouldBeOfType<EmptyNode>();
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
            Parser target = new($@"\{metaCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(metaCharacter);
        }

        [TestMethod]
        public void ParsingBackslashUppercaseAShouldReturnStartOfStringNode()
        {
            // Arrange
            Parser target = new(@"\A");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<StartOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseZShouldReturnEndOfStringZNode()
        {
            // Arrange
            Parser target = new(@"\Z");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<EndOfStringZNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseZShouldReturnEndOfStringNode()
        {
            // Arrange
            Parser target = new(@"\z");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<EndOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseBShouldReturnWordBoundaryNode()
        {
            // Arrange
            Parser target = new(@"\b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<WordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseBShouldReturnNonWordBoundaryNode()
        {
            // Arrange
            Parser target = new(@"\B");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<NonWordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseGShouldReturnContiguousMatchNode()
        {
            // Arrange
            Parser target = new(@"\G");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<ContiguousMatchNode>();
        }

        [TestMethod]
        public void ParsingCaretShouldReturnStartOfLineNode()
        {
            // Arrange
            Parser target = new("^");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<StartOfLineNode>();
        }

        [TestMethod]
        public void ParsingDollarShouldReturnEndOfLineNode()
        {
            // Arrange
            Parser target = new("$");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<EndOfLineNode>();
        }

        [TestMethod]
        public void ParsingDotShouldReturnAnyCharacterNode()
        {
            // Arrange
            Parser target = new(".");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<AnyCharacterNode>();
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
            Parser target = new($@"\{shorthandCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassShorthandNode characterClassShorthandNode = childNode.ShouldBeOfType<CharacterClassShorthandNode>();
            characterClassShorthandNode.Shorthand.ShouldBe(shorthandCharacter);
        }

        [TestMethod]
        public void ParsingBackslashLowercasePUnicodeCategoryShouldReturnUnicodeCategoryNodeWithRightCategory()
        {
            // Arrange
            string category = "IsBasicLatin";
            Parser target = new($@"\p{{{category}}}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            UnicodeCategoryNode unicodeCategoryNode = childNode.ShouldBeOfType<UnicodeCategoryNode>();
            unicodeCategoryNode.Category.ShouldBe(category);
            unicodeCategoryNode.Negated.ShouldBe(false);
        }

        [TestMethod]
        public void ParsingBackslashUppercasePUnicodeCategoryShouldReturnNegatedUnicodeCategoryNodeWithRightCategory()
        {
            // Arrange
            string category = "IsBasicLatin";
            Parser target = new($@"\P{{{category}}}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
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
            Parser target = new($@"\{escape}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
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
            Parser target = new($@"\{hexCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
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
            Parser target = new($@"\{unicodeCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
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
            Parser target = new($@"\{controlCharacter}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            EscapeCharacterNode escapeNode = childNode.ShouldBeOfType<EscapeCharacterNode>();
            escapeNode.Escape.ShouldBe(controlCharacter);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(9)]
        public void ParsingBackslashDigitShouldReturnBackreferenceNodeWithGroupNumber(int groupNumber)
        {
            // Arrange
            Parser target = new($@"()()()()()()()()()\{groupNumber}");

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
            Parser target = new($@"(?<name>)\k<{name}>");

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
            Parser target = new($@"(?<name>)\k'{name}'");

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
            Parser target = new($@"(?<name>)\<{name}>");

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
            Parser target = new($@"(?<name>)\'{name}'");

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
            Parser target = new("()");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CaptureGroupNode captureGroupNode = childNode.ShouldBeOfType<CaptureGroupNode>();
            RegexNode groupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingParenthesesWithCharactersShouldReturnCaptureGroupWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<CaptureGroupNode>();
            RegexNode captureGroupchildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = captureGroupchildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingParenthesesWithAlternationShouldReturnCaptureGroupWithAlternationNode()
        {
            // Arrange
            Parser target = new("(a|b|c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<CaptureGroupNode>();
            RegexNode captureGroupchildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            AlternationNode alternationNode = captureGroupchildNode.ShouldBeOfType<AlternationNode>();
            alternationNode.ChildNodes.Count().ShouldBe(3);
            ConcatenationNode alternate = alternationNode.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            RegexNode alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            _ = alternateChildNode.ShouldBeOfType<CharacterNode>();
            alternate = alternationNode.ChildNodes.ElementAt(1).ShouldBeOfType<ConcatenationNode>();
            alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            _ = alternateChildNode.ShouldBeOfType<CharacterNode>();
            alternate = alternationNode.ChildNodes.ElementAt(2).ShouldBeOfType<ConcatenationNode>();
            alternateChildNode = alternate.ChildNodes.ShouldHaveSingleItem();
            _ = alternateChildNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingMultipleParenthesesShouldReturnMultipleCaptureGroupNodes()
        {
            // Arrange
            Parser target = new("(a)b(c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(3);
            CaptureGroupNode captureGroupNode = root.ChildNodes.First().ShouldBeOfType<CaptureGroupNode>();
            RegexNode captureGroupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = captureGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            RegexNode concatentationChildNode = concatenationNode.ChildNodes.ShouldHaveSingleItem();
            _ = concatentationChildNode.ShouldBeOfType<CharacterNode>();

            _ = root.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();

            captureGroupNode = root.ChildNodes.Last().ShouldBeOfType<CaptureGroupNode>();
            captureGroupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            concatenationNode = captureGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatentationChildNode = concatenationNode.ChildNodes.ShouldHaveSingleItem();
            _ = concatentationChildNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNestedParenthesesShouldReturnNestedCaptureGroupNodes()
        {
            // Arrange
            Parser target = new("(a(b)c)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CaptureGroupNode captureGroupNode = childNode.ShouldBeOfType<CaptureGroupNode>();
            RegexNode captureGroupChild = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = captureGroupChild.ShouldBeOfType<ConcatenationNode>();
            captureGroupChild.ChildNodes.Count().ShouldBe(3);
            _ = captureGroupChild.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = captureGroupChild.ChildNodes.Last().ShouldBeOfType<CharacterNode>();

            CaptureGroupNode nestedGroup = captureGroupChild.ChildNodes.ElementAt(1).ShouldBeOfType<CaptureGroupNode>();
            RegexNode nestedGroupChildNode = nestedGroup.ChildNodes.ShouldHaveSingleItem();
            _ = nestedGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            RegexNode nestedGroupCharacterNode = nestedGroupChildNode.ChildNodes.ShouldHaveSingleItem();
            _ = nestedGroupCharacterNode.ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithNameBetweenAngledBracketsShouldReturnNamedGroupNodeWithUseQuotesIsFalse()
        {
            // Arrange
            Parser target = new("(?<name>)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            NamedGroupNode namedGroupNode = childNode.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe("name");
            namedGroupNode.UseQuotes.ShouldBeFalse();
            RegexNode groupChildNode = namedGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithNameBetweenSingleQuotesShouldReturnNamedGroupNodeWithUseQuotesIsTrue()
        {
            // Arrange
            Parser target = new("(?'name')");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            NamedGroupNode namedGroupNode = childNode.ShouldBeOfType<NamedGroupNode>();
            namedGroupNode.Name.ShouldBe("name");
            namedGroupNode.UseQuotes.ShouldBeTrue();
            RegexNode groupChildNode = namedGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNamedGroupWithCharactersShouldReturnNamedGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?<name>abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<NamedGroupNode>();
            RegexNode groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNonCaptureGroupShouldReturNonCaptureGroupNode()
        {
            // Arrange
            Parser target = new("(?:)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<NonCaptureGroupNode>();
            RegexNode groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNonCaptureGroupWithCharactersShouldReturnNonCaptureGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?:abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<NonCaptureGroupNode>();
            RegexNode groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingAtomicGroupShouldReturnAtomicGroupNode()
        {
            // Arrange
            Parser target = new("(?>)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<AtomicGroupNode>();
            RegexNode groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingAtomicGroupWithCharactersShouldReturnAtomicGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?>abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<AtomicGroupNode>();
            RegexNode groupChildNode = childNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookaheadGroupShouldReturnLookaroundGroupNodeWithLookaheadIsTrueAndPossitiveIsTrue()
        {
            // Arrange
            Parser target = new("(?=)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookaheadGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?=abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookaheadGroupShouldReturnLookaroundGroupNodeWithLookaheadIsTrueAndPossitiveIsFalse()
        {
            // Arrange
            Parser target = new("(?!)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookaheadGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?!abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeTrue();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookbehindGroupShouldReturnLookaroundGroupNodeWithLookaheadIsFalseAndPossitiveIsTrue()
        {
            // Arrange
            Parser target = new("(?<=)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingPossitiveLookbehindGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?<=abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeTrue();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookbehindGroupShouldReturnLookaroundGroupNodeWithLookaheadIsFalseAndPossitiveIsFalse()
        {
            // Arrange
            Parser target = new("(?<!)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = groupChildNode.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingNegativeLookbehindGroupWithCharactersShouldReturnLookaroundGroupNodeWithConcatenationNode()
        {
            // Arrange
            Parser target = new("(?<!abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LookaroundGroupNode lookaroundGroupNode = childNode.ShouldBeOfType<LookaroundGroupNode>();
            lookaroundGroupNode.Lookahead.ShouldBeFalse();
            lookaroundGroupNode.Possitive.ShouldBeFalse();
            RegexNode groupChildNode = lookaroundGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = groupChildNode.ShouldBeOfType<ConcatenationNode>();
            concatenationNode.ChildNodes.Count().ShouldBe(3);
            _ = concatenationNode.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            _ = concatenationNode.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [TestMethod]
        public void ParsingConditionalGroupShouldReturnConditionalGroupNodeWithGroupNodeAsFirstChildAndAlternationWithTwoAlternatesAsSecondChild()
        {
            // Arrange
            Parser target = new("(?(condition)then|else)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            ConditionalGroupNode conditionalGroupNode = childNode.ShouldBeOfType<ConditionalGroupNode>();
            conditionalGroupNode.ChildNodes.Count().ShouldBe(2);

            _ = conditionalGroupNode.ChildNodes.First().ShouldBeOfType<CaptureGroupNode>();
            RegexNode condition = conditionalGroupNode.ChildNodes.First().ChildNodes.ShouldHaveSingleItem();
            condition.ToString().ShouldBe("condition");

            AlternationNode alternation = conditionalGroupNode.ChildNodes.Last().ShouldBeOfType<AlternationNode>();
            alternation.ChildNodes.Count().ShouldBe(2);
            alternation.ChildNodes.First().ToString().ShouldBe("then");
            alternation.ChildNodes.Last().ToString().ShouldBe("else");
        }

        [TestMethod]
        public void ParsingModeModifierGroupWithoutSubexpressionShouldReturnModeModifierGroupNodeWithModesAndEmotyNodeAsChildNode()
        {
            // Arrange
            Parser target = new("(?imnsx-imnsx+imnsx)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imnsx-imnsx+imnsx");
            RegexNode empty = modeModifierGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = empty.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void ParsingModeModifierGroupWithSubexpressionAfterColonShouldReturnModeModifierGroupNodeWithModesAndChildNode()
        {
            // Arrange
            Parser target = new("(?imnsx-imnsx+imnsx:abc)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imnsx-imnsx+imnsx");
            RegexNode modeModifierChildNode = modeModifierGroupNode.ChildNodes.ShouldHaveSingleItem();
            _ = modeModifierChildNode.ShouldBeOfType<ConcatenationNode>();
            modeModifierChildNode.ChildNodes.Count().ShouldBe(3);
            modeModifierChildNode.ToString().ShouldBe("abc");
        }

        [TestMethod]
        public void UsingSameModeModifierMultipleTimesShouldBeAllowed()
        {
            // Arrange
            Parser target = new("(?iimmnnssxxxx-iimmnnssxxxx+iimmnnssxxxx)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("iimmnnssxxxx-iimmnnssxxxx+iimmnnssxxxx");
        }

        [TestMethod]
        public void UsingMinusAndPlusMultipleTimesInModeModfierGroupShouldBeAllowed()
        {
            // Arrange
            Parser target = new("(?imn-s+x-im+-+ns-x+im)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            ModeModifierGroupNode modeModifierGroupNode = childNode.ShouldBeOfType<ModeModifierGroupNode>();
            modeModifierGroupNode.Modifiers.ShouldBe("imn-s+x-im+-+ns-x+im");
        }

        [TestMethod]
        public void ParsingBalancingGroupWithOneNameInAngledBracketsShouldReturnBalancingGroupWithBalancedGroupNameAndUseQuotesFalse()
        {
            // Arrange
            Parser target = new("(?<balancedGroup>)(?<-balancedGroup>)");

            // Act
            RegexTree result = target.Parse();

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
            Parser target = new("(?<balancedGroup>)(?<name-balancedGroup>)");

            // Act
            RegexTree result = target.Parse();

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
            Parser target = new("(?<balancedGroup>)(?'-balancedGroup')");

            // Act
            RegexTree result = target.Parse();

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
            Parser target = new("(?<balancedGroup>)(?'name-balancedGroup')");

            // Act
            RegexTree result = target.Parse();

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
            Parser target = new("a?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void QuantifierQuestionMarkCanBeLazy()
        {
            // Arrange
            Parser target = new("a??");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void ParsingPlusShouldReturnQuantifierPlusNode()
        {
            // Arrange
            Parser target = new("a+");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<QuantifierPlusNode>();
        }

        [TestMethod]
        public void QuantifierPlusCanBeLazy()
        {
            // Arrange
            Parser target = new("a+?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierPlusNode>();
        }

        [TestMethod]
        public void ParsingStarShouldReturnQuantifierStarNode()
        {
            // Arrange
            Parser target = new("a*");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            _ = childNode.ShouldBeOfType<QuantifierStarNode>();
        }

        [TestMethod]
        public void QuantifierStarCanBeLazy()
        {
            // Arrange
            Parser target = new("a*?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierStarNode>();
        }

        [TestMethod]
        public void ParsingIntegerBetweenCurlyBracketsShouldReturnQuantifierNNodeWithIntegerAsN()
        {
            // Arrange
            Parser target = new("a{5}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNNode quantifierNode = childNode.ShouldBeOfType<QuantifierNNode>();
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void QuantifierNCanBeLazy()
        {
            // Arrange
            Parser target = new("a{5}?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierNNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesBetweenCurlyBracketsShouldReturnQuantifierNNodeWithLeadingZeroes()
        {
            // Arrange
            Parser target = new("a{05}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNNode quantifierNode = childNode.ShouldBeOfType<QuantifierNNode>();
            quantifierNode.OriginalN.ShouldBe("05");
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void ParsingIntegerCommaBetweenCurlyBracketsShouldReturnQuantifierNNodeWithIntegerAsN()
        {
            // Arrange
            Parser target = new("a{5,}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNOrMoreNode quantifierNode = childNode.ShouldBeOfType<QuantifierNOrMoreNode>();
            quantifierNode.N.ShouldBe(5);
        }

        [TestMethod]
        public void QuantifierNOrMoreCanBeLazy()
        {
            // Arrange
            Parser target = new("a{5,}?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierNOrMoreNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesCommaBetweenCurlyBracketsShouldReturnQuantifierNNodeWithLeadingZeroes()
        {
            // Arrange
            Parser target = new("a{05,}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNOrMoreNode quantifierNode = childNode.ShouldBeOfType<QuantifierNOrMoreNode>();
            quantifierNode.OriginalN.ShouldBe("05");
        }

        [TestMethod]
        public void ParsingIntegerCommaIntegerBetweenCurlyBracketsShouldReturnQuantifierNMNodeIfNIsLessThanM()
        {
            // Arrange
            Parser target = new("a{9,10}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNMNode quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
            quantifierNode.N.ShouldBe(9);
            quantifierNode.M.ShouldBe(10);
        }

        [TestMethod]
        public void ParsingIntegerNCommaIntegerMBetweenCurlyBracketsShouldReturnQuantifierNMNodeIfNIsEqualToM()
        {
            // Arrange
            Parser target = new("a{10,10}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNMNode quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
            quantifierNode.N.ShouldBe(10);
            quantifierNode.M.ShouldBe(10);
        }

        [TestMethod]
        public void ParsingIntegerNCommaIntegerMBetweenCurlyBracketsShouldThrowRegexParseExceptionIfNIsGreaterThanM()
        {
            // Act
            Action act = () =>
            {
                Parser target = new("a{11,10}");
                _ = target.Parse();
            };

            // Assert
            _ = act.ShouldThrow<RegexParseException>();

        }

        [TestMethod]
        public void QuantifierNMCanBeLazy()
        {
            // Arrange
            Parser target = new("a{5,10}?");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            LazyNode lazyNode = childNode.ShouldBeOfType<LazyNode>();
            RegexNode quantifierNode = lazyNode.ChildNodes.ShouldHaveSingleItem();
            _ = quantifierNode.ShouldBeOfType<QuantifierNMNode>();
        }

        [TestMethod]
        public void ParsingIntegerWithLeadingZeroesCommaIntegerWithLeadingZeroesBetweenCurlyBracketsShouldReturnQuantifierNNodeWithWithLeadingZeroes()
        {
            // Arrange
            Parser target = new("a{05,010}");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            QuantifierNMNode quantifierNode = childNode.ShouldBeOfType<QuantifierNMNode>();
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
            Parser target = new(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            CharacterNode characterNode = root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
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
            Parser target = new(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            _ = root.ChildNodes.Last().ShouldBeOfType<QuantifierQuestionMarkNode>();
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
            Parser target = new(pattern);

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.ShouldNotBeEmpty();
            _ = root.ChildNodes.Last().ShouldBeOfType<QuantifierQuestionMarkNode>();
        }

        [TestMethod]
        public void ParsingCharactersBetweenSquareBracketsNotStartingWithCaretShouldReturnCharacterClassWithNegationFalseAndCharactersInCharacterSet()
        {
            // Arrange
            Parser target = new("[abc]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeFalse();
            RegexNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            _ = characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            CharacterNode characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void ParsingCharactersBetweenSquareBracketsStartingWithCaretShouldReturnCharacterClassWithNegationTrue()
        {
            // Arrange
            Parser target = new("[^abc]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeTrue();
            RegexNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            _ = characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            CharacterNode characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void DashAtStartOfCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            Parser target = new("[-abc]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            RegexNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            _ = characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            CharacterNode characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterStartingCaretInCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            Parser target = new("[^-abc]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.Negated.ShouldBeTrue();
            RegexNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            _ = characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            CharacterNode characterNode = characterSet.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAtEndOfCharacterClassShouldBeParsedAsCharacterNode()
        {
            // Arrange
            Parser target = new("[abc-]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            RegexNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem();
            _ = characterSet.ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(4);
            CharacterNode characterNode = characterSet.ChildNodes.Last().ShouldBeOfType<CharacterNode>();
            characterNode.ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashBeforeCharacterClassInsideCharacterClassShouldBeParsedAsCharacterClassSubtraction()
        {
            // Arrange
            Parser target = new("[abc-[a]]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.ChildNodes.Count().ShouldBe(2);

            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);

            CharacterClassNode subtraction = characterClassNode.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
            RegexNode subtractionCharacterSet = subtraction.ChildNodes.ShouldHaveSingleItem();
            RegexNode subtractedCharacter = subtractionCharacterSet.ChildNodes.ShouldHaveSingleItem();
            _ = subtractedCharacter.ShouldBeOfType<CharacterNode>();
            subtractedCharacter.ToString().ShouldBe("a");
        }

        [TestMethod]
        public void CharacterClassSubtractionCanBeNested()
        {
            // Arrange
            Parser target = new("[abc-[ab-[c]]]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            RegexNode childNode = root.ChildNodes.ShouldHaveSingleItem();
            CharacterClassNode characterClassNode = childNode.ShouldBeOfType<CharacterClassNode>();
            characterClassNode.ChildNodes.Count().ShouldBe(2);

            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);

            CharacterClassNode subtraction = characterClassNode.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
            subtraction.ChildNodes.Count().ShouldBe(2);

            CharacterClassCharacterSetNode subtractionCharacterSet = subtraction.ChildNodes.First().ShouldBeOfType<CharacterClassCharacterSetNode>();
            subtractionCharacterSet.ChildNodes.Count().ShouldBe(2);


            CharacterClassNode nestedSubtraction = subtraction.ChildNodes.Last().ShouldBeOfType<CharacterClassNode>();
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
            Parser target = new($@"[\{shorthand}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            CharacterClassShorthandNode shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassShorthandNode>();
            shorthandNode.Shorthand.ShouldBe(shorthand);
        }

        [TestMethod]
        public void UnicodeCategoryIsValidInCharacterClass()
        {
            // Arrange
            string category = "IsBasicLatin";
            Parser target = new($@"[\p{{{category}}}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            UnicodeCategoryNode shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<UnicodeCategoryNode>();
            shorthandNode.Category.ShouldBe(category);
            shorthandNode.Negated.ShouldBeFalse();
        }

        [TestMethod]
        public void NegatedUnicodeCategoryIsValidInCharacterClass()
        {
            // Arrange
            string category = "IsBasicLatin";
            Parser target = new($@"[\P{{{category}}}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            UnicodeCategoryNode shorthandNode = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<UnicodeCategoryNode>();
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
            Parser target = new($@"[\{escapedChar}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
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
            Parser target = new($@"[\{escapedChar}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
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
            Parser target = new($@"[\{hex}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
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
            Parser target = new($@"[\{hex}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
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
            Parser target = new($@"[\{oct}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
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
            Parser target = new($@"[\{oct}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe(oct);
        }

        [TestMethod]
        public void EscapeCharacterIsValidInTheMiddleOfACharacterClass()
        {
            // Arrange
            Parser target = new($@"[a\nb]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe("n");
        }

        [TestMethod]
        public void EscapeCharacterIsValidAtTheEndOfACharacterClass()
        {
            // Arrange
            Parser target = new($@"[a\n]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(2);
            EscapeCharacterNode escapeChar = characterSet.ChildNodes.Last().ShouldBeOfType<EscapeCharacterNode>();
            escapeChar.Escape.ShouldBe("n");
        }

        [TestMethod]
        [DataRow("a", "z")]
        [DataRow("a", "a")]
        public void DashBetweenCharactersInCharacterClassShouldBeRange(string start, string end)
        {
            // Arrange
            Parser target = new($"[{start}-{end}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            CharacterClassRangeNode range = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassRangeNode>();
            range.ChildNodes.Count().ShouldBe(2);
            range.ChildNodes.First().ShouldBeOfType<CharacterNode>().ToString().ShouldBe(start);
            range.ChildNodes.Last().ShouldBeOfType<CharacterNode>().ToString().ShouldBe(end);
        }

        [TestMethod]
        public void DashAfterCharacterClassShorthandInCharacterClassShouldBeLiteral()
        {
            // Arrange
            Parser target = new(@"[\d-z]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterUnicodeCategoryInCharacterClassShouldBeLiteral()
        {
            // Arrange
            Parser target = new(@"[\p{IsBasicLatin}-z]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        public void DashAfterRangeInCharacterClassShouldBeLiteral()
        {
            // Arrange
            Parser target = new(@"[a-z-z]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            characterSet.ChildNodes.Count().ShouldBe(3);
            characterSet.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>().ToString().ShouldBe("-");
        }

        [TestMethod]
        [DataRow("{", "}")]
        [DataRow("{", "{")]
        public void DashBetweenEscapeCharactersInCharacterClassShouldBeRange(string start, string end)
        {
            // Arrange
            Parser target = new($@"[\{start}-\{end}]");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterClassNode characterClassNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassNode>();
            CharacterClassCharacterSetNode characterSet = characterClassNode.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassCharacterSetNode>();
            CharacterClassRangeNode range = characterSet.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterClassRangeNode>();
            range.ChildNodes.Count().ShouldBe(2);
            range.ChildNodes.First().ShouldBeOfType<EscapeCharacterNode>().Escape.ShouldBe(start);
            range.ChildNodes.Last().ShouldBeOfType<EscapeCharacterNode>().Escape.ShouldBe(end);
        }

        [TestMethod]
        public void ParsingCommentGroupShouldAddCommentAsPrefixForNextToken()
        {
            // Arrange
            Parser target = new("(?#This is a comment.)a");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterNode characterNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            CommentGroupNode comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfPatternShouldAddCommentAsPrefixForEmptyNode()
        {
            // Arrange
            Parser target = new("a(?#This is a comment.)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            EmptyNode emptyNode = root.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingPatternWithOnlyCommentGroupShouldReturnOnlyEmptyNodeWithPrefix()
        {
            // Arrange
            Parser target = new("(?#This is a comment.)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root.ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = root.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupInGroupShouldAddCommentAsPrefixForNextTokenInGroup()
        {
            // Arrange
            Parser target = new("((?#This is a comment.)a)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CaptureGroupNode group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            ConcatenationNode groupConcat = group.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<ConcatenationNode>();
            CharacterNode characterNode = groupConcat.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            CommentGroupNode comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfAGroupShouldAddCommentAsPrefixForEmptyNodeAtTheEndOfTheGroup()
        {
            // Arrange
            Parser target = new("(a(?#This is a comment.))");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CaptureGroupNode group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            RegexNode groupConcatenation = group.ChildNodes.ShouldHaveSingleItem();
            groupConcatenation.ChildNodes.Count().ShouldBe(2);
            EmptyNode emptyNode = groupConcatenation.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingGroupWithOnlyCommentGroupShouldAddCommentAsPrefixForEmptyNodeAsOnlyChildOfTheGroup()
        {
            // Arrange
            Parser target = new("((?#This is a comment.))");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CaptureGroupNode group = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CaptureGroupNode>();
            EmptyNode emptyNode = group.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupInAlternationShouldAddCommentAsPrefixForNextToken()
        {
            // Arrange
            Parser target = new("(?#This is a comment.)a|b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            AlternationNode root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            ConcatenationNode alternate = root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            CharacterNode characterNode = alternate.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            CommentGroupNode comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupAtEndOfAlternateShouldAddCommentAsPrefixForEmptyNode()
        {
            // Arrange
            Parser target = new("a(?#This is a comment.)|b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            AlternationNode root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            ConcatenationNode alternate = root.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            alternate.ChildNodes.Count().ShouldBe(2);
            EmptyNode emptyNode = alternate.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = emptyNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingAlternateWithOnlyCommentGroupShouldAddOnlyEmptyNodeWithPrefixAsAlternate()
        {
            // Arrange
            Parser target = new("(?#This is a comment.)|b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            AlternationNode root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            EmptyNode alternate = root.ChildNodes.First().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = alternate.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingAlternationWithOnlyCommentGroupInLastAlternateShouldAddOnlyEmptyNodeWithPrefixAsAlternate()
        {
            // Arrange
            Parser target = new("a|(?#This is a comment.)");

            // Act
            RegexTree result = target.Parse();

            // Assert
            AlternationNode root = result.Root.ShouldBeOfType<AlternationNode>();
            root.ChildNodes.Count().ShouldBe(2);
            EmptyNode alternate = root.ChildNodes.Last().ShouldBeOfType<EmptyNode>();
            CommentGroupNode comment = alternate.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is a comment.");
        }

        [TestMethod]
        public void ParsingMultipleCommentGroupsOnMultipleTokensShouldAddCommentsAsPrefixesForNextTokens()
        {
            // Arrange
            Parser target = new("(?#This is the first comment.)a(?#This is the second comment.)b");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            root.ChildNodes.Count().ShouldBe(2);
            CharacterNode firstCharacter = root.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            CommentGroupNode firstComment = firstCharacter.Prefix.ShouldBeOfType<CommentGroupNode>();
            firstComment.Comment.ShouldBe("This is the first comment.");
            CharacterNode lastCharacter = root.ChildNodes.Last().ShouldBeOfType<CharacterNode>();
            CommentGroupNode lastComment = lastCharacter.Prefix.ShouldBeOfType<CommentGroupNode>();
            lastComment.Comment.ShouldBe("This is the second comment.");
        }

        [TestMethod]
        public void ParsingCommentGroupBeforeCommentGroupShouldAddCommentAsPrefixForNextCommentGroup()
        {
            // Arrange
            Parser target = new("(?#This is the first comment.)(?#This is the second comment.)(?#This is the third comment.)a");

            // Act
            RegexTree result = target.Parse();

            // Assert
            RegexNode root = result.Root;
            CharacterNode characterNode = root.ChildNodes.ShouldHaveSingleItem().ShouldBeOfType<CharacterNode>();
            CommentGroupNode comment = characterNode.Prefix.ShouldBeOfType<CommentGroupNode>();
            comment.Comment.ShouldBe("This is the third comment.");
            CommentGroupNode secondComment = comment.Prefix.ShouldBeOfType<CommentGroupNode>();
            secondComment.Comment.ShouldBe("This is the second comment.");
            CommentGroupNode lastComment = secondComment.Prefix.ShouldBeOfType<CommentGroupNode>();
            lastComment.Comment.ShouldBe("This is the first comment.");
        }
    }
}
