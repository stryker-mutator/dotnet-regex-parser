﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexParser.Nodes;
using RegexParser.Nodes.AnchorNodes;
using RegexParser.Nodes.GroupNodes;
using Shouldly;
using System;
using System.Linq;

namespace RegexParser.UnitTest
{
    [TestClass]
    public class ParserTest
    {
        [DataTestMethod]
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
        public void ParseShouldReturnRegexNodeWithOriginalRegexPattern(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            var result = target.Parse();

            // Assert
            result.ToString().ShouldBe(pattern);
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
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<EmptyNode>();
        }

        [TestMethod]
        public void CharacterNodesAreAddedToConcatenationNode()
        {
            // Arrange
            var target = new Parser("abc");

            // Act
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<ConcatenationNode>();
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.First().ShouldBeOfType<CharacterNode>();
            result.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();
            result.ChildNodes.ElementAt(2).ShouldBeOfType<CharacterNode>();
        }

        [DataTestMethod]
        [DataRow("a|b|c")]
        [DataRow("a1|b2|c3")]
        public void ConcatenationNodesAreAddedToAlternationNode(string pattern)
        {
            // Arrange
            var target = new Parser(pattern);

            // Act
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<AlternationNode>();
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.First().ShouldBeOfType<ConcatenationNode>();
            result.ChildNodes.ElementAt(1).ShouldBeOfType<ConcatenationNode>();
            result.ChildNodes.ElementAt(2).ShouldBeOfType<ConcatenationNode>();
        }

        [DataTestMethod]
        public void EmptyFirstAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("|b|c");

            // Act
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<AlternationNode>();
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.First().ShouldBeOfType<EmptyNode>();
        }

        [DataTestMethod]
        public void EmptyMiddleAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("a||c");

            // Act
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<AlternationNode>();
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.ElementAt(1).ShouldBeOfType<EmptyNode>();
        }

        [DataTestMethod]
        public void EmptyLastAlternateInAlternationShouldBeEmptyNode()
        {
            // Arrange
            var target = new Parser("a|b|");

            // Act
            RegexNode result = target.Parse();

            // Assert
            result.ShouldBeOfType<AlternationNode>();
            result.ChildNodes.Count().ShouldBe(3);
            result.ChildNodes.ElementAt(2).ShouldBeOfType<EmptyNode>();
        }

        [DataTestMethod]
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            var escapeNode = childNode.ShouldBeOfType<EscapeNode>();
            escapeNode.Escape.ShouldBe(metaCharacter);
        }

        [TestMethod]
        public void ParsingBackslashUppercaseAShouldReturnStartOfStringNode()
        {
            // Arrange
            var target = new Parser(@"\A");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<StartOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseZShouldReturnEndOfStringZNode()
        {
            // Arrange
            var target = new Parser(@"\Z");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfStringZNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseZShouldReturnEndOfStringNode()
        {
            // Arrange
            var target = new Parser(@"\z");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfStringNode>();
        }

        [TestMethod]
        public void ParsingBackslashLowercaseBShouldReturnWordBoundaryNode()
        {
            // Arrange
            var target = new Parser(@"\b");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<WordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseBShouldReturnNonWordBoundaryNode()
        {
            // Arrange
            var target = new Parser(@"\B");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<NonWordBoundaryNode>();
        }

        [TestMethod]
        public void ParsingBackslashUppercaseGShouldReturnContiguousMatchNode()
        {
            // Arrange
            var target = new Parser(@"\G");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<ContiguousMatchNode>();
        }

        [TestMethod]
        public void ParsingCaretShouldReturnStartOfLineNode()
        {
            // Arrange
            var target = new Parser("^");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<StartOfLineNode>();
        }

        [TestMethod]
        public void ParsingDollarShouldReturnEndOfLineNode()
        {
            // Arrange
            var target = new Parser("$");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<EndOfLineNode>();
        }

        [TestMethod]
        public void ParsingDotShouldReturnAnyCharacterNode()
        {
            // Arrange
            var target = new Parser(".");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            childNode.ShouldBeOfType<AnyCharacterNode>();
        }

        [DataTestMethod]
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            UnicodeCategoryNode unicodeCategoryNode = childNode.ShouldBeOfType<UnicodeCategoryNode>();
            unicodeCategoryNode.Category.ShouldBe(category);
            unicodeCategoryNode.Negated.ShouldBe(true);
        }

        [DataTestMethod]
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            EscapeNode escapeNode = childNode.ShouldBeOfType<EscapeNode>();
            escapeNode.Escape.ShouldBe(escape);
        }

        [DataTestMethod]
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            EscapeNode escapeNode = childNode.ShouldBeOfType<EscapeNode>();
            escapeNode.Escape.ShouldBe(hexCharacter);
        }

        [DataTestMethod]
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            EscapeNode escapeNode = childNode.ShouldBeOfType<EscapeNode>();
            escapeNode.Escape.ShouldBe(unicodeCharacter);
        }

        [DataTestMethod]
        [DataRow("cA")]
        [DataRow("cZ")]
        [DataRow("ca")]
        [DataRow("cz")]
        public void ParsingBackslashLowercaseCAlphaShouldReturnEscapecharacterNodeWithEscapeCharacter(string controlCharacter)
        {
            // Arrange
            var target = new Parser($@"\{controlCharacter}");

            // Act
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
            EscapeNode escapeNode = childNode.ShouldBeOfType<EscapeNode>();
            escapeNode.Escape.ShouldBe(controlCharacter);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        public void ParsingBackslashDigitShouldReturnBackreferenceNodeWithGroupNumber(int groupNumber)
        {
            // Arrange
            var target = new Parser($@"()\{groupNumber}");

            // Act
            RegexNode result = target.Parse();

            // Assert
            BackreferenceNode backreferenceNode = result.ChildNodes.Last().ShouldBeOfType<BackreferenceNode>();
            backreferenceNode.GroupNumber.ShouldBe(groupNumber);
        }

        [DataTestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashLowercaseKNameBetweenAngledBracketsShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsFalseAndUseKIsTrue(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\k<{name}>");

            // Act
            RegexNode result = target.Parse();

            // Assert
            NamedReferenceNode namedReferenceNode = result.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(false);
            namedReferenceNode.UseK.ShouldBe(true);
        }

        [DataTestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashLowercaseKNameBetweenSingleQuotesShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsTrueAndUseKIsTrue(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\k'{name}'");

            // Act
            RegexNode result = target.Parse();

            // Assert
            NamedReferenceNode namedReferenceNode = result.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(true);
            namedReferenceNode.UseK.ShouldBe(true);
        }

        [DataTestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashNameBetweenAngledBracketsShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsFalseAndUseKIsFalse(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\<{name}>");

            // Act
            RegexNode result = target.Parse();

            // Assert
            NamedReferenceNode namedReferenceNode = result.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
            namedReferenceNode.Name.ShouldBe(name);
            namedReferenceNode.UseQuotes.ShouldBe(false);
            namedReferenceNode.UseK.ShouldBe(false);
        }

        [DataTestMethod]
        [DataRow("name")]
        [DataRow("1")]
        public void ParsingBackslashNameBetweenSingleQuotesShouldReturnNamedReferenceNodeWithNameAndUseQuotesIsTrueAndUseKIsFalse(string name)
        {
            // Arrange
            var target = new Parser($@"(?<name>)\'{name}'");

            // Act
            RegexNode result = target.Parse();

            // Assert
            NamedReferenceNode namedReferenceNode = result.ChildNodes.Last().ShouldBeOfType<NamedReferenceNode>();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            result.ChildNodes.Count().ShouldBe(3);
            CaptureGroupNode captureGroupNode = result.ChildNodes.First().ShouldBeOfType<CaptureGroupNode>();
            var captureGroupChildNode = captureGroupNode.ChildNodes.ShouldHaveSingleItem();
            ConcatenationNode concatenationNode = captureGroupChildNode.ShouldBeOfType<ConcatenationNode>();
            var concatentationChildNode = concatenationNode.ChildNodes.ShouldHaveSingleItem();
            concatentationChildNode.ShouldBeOfType<CharacterNode>();

            result.ChildNodes.ElementAt(1).ShouldBeOfType<CharacterNode>();

            captureGroupNode = result.ChildNodes.Last().ShouldBeOfType<CaptureGroupNode>();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            RegexNode result = target.Parse();

            // Assert
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
            var childNode = result.ChildNodes.ShouldHaveSingleItem();
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
    }
}
