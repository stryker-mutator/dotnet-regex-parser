using Stryker.Regex.Parser.Exceptions;
using Stryker.Regex.Parser.Nodes;
using Stryker.Regex.Parser.Nodes.AnchorNodes;
using Stryker.Regex.Parser.Nodes.CharacterClass;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using Stryker.Regex.Parser.Nodes.QuantifierNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RegexParseError = Stryker.Regex.Parser.Exceptions.RegexParseError;
using RegexParseException = Stryker.Regex.Parser.Exceptions.RegexParseException;

namespace Stryker.Regex.Parser;

public class Parser : IParser
{
    private const int HexadecimalEscapeDigits = 2;
    private const int UnicodeEscapeDigits = 4;
    private const int MaxDigitsInOctalEscape = 3;
    private const int MaxAlternatesInConditionalGroup = 2;

    private int _currentPosition;
    private bool _previousWasQuantifier;
    private readonly List<RegexNode> _alternates = [];
    private readonly List<RegexNode> _concatenation = [];
    private readonly Stack<GroupUnit> _groupStack = new();
    private GroupUnit _group;
    private CommentGroupNode _prefix;
    private readonly UnicodeCategory[] _wordCharCategories = [
        UnicodeCategory.UppercaseLetter,
        UnicodeCategory.LowercaseLetter,
        UnicodeCategory.TitlecaseLetter,
        UnicodeCategory.ModifierLetter,
        UnicodeCategory.OtherLetter,
        UnicodeCategory.NonSpacingMark,
        UnicodeCategory.SpacingCombiningMark,
        UnicodeCategory.DecimalDigitNumber,
        UnicodeCategory.ConnectorPunctuation
    ];

    public string Pattern { get; }

    public Parser(string pattern)
    {
        IsValidRegex(pattern);
        Pattern = pattern;
    }

    private static void IsValidRegex(string pattern)
    {
        try
        {
            _ = new System.Text.RegularExpressions.Regex(pattern);
        }

        catch (ArgumentException ex)
        {
            throw new RegexParseException(ex.Message);
        }
    }

    /// <summary>
    /// Builds a tree of RegexNodes from a regular expression.
    /// </summary>
    public RegexTree Parse()
    {
        while (CharsRight() > 0)
        {
            // Move past all literal characters and add them to the current concatenation item.
            ParseLiteralCharacters();

            if (CharsRight() > 0)
            {
                ParseNonLiteralCharacter();
            }
        }

        // A group was started with "(", but never closed with ")".
        if (_groupStack.Any())
        {
            throw MakeException(RegexParseError.NotEnoughParentheses);
        }

        RegexNode root = CreateOuterNode();
        return new RegexTree(root);
    }

    /// <summary>
    /// Parse all characters as literal characters and add them to the current concatenation item until we find a special character.
    /// </summary>
    private void ParseLiteralCharacters()
    {
        int startPosition = _currentPosition;

        while (CharsRight() > 0 && !IsSpecial(RightChar()))
        {
            char ch = RightCharMoveRight();
            AddNode(new CharacterNode(ch));
        }

        if (_currentPosition > startPosition)
        {
            _previousWasQuantifier = false;
        }
    }

    /// <summary>
    /// Parse anything that is not a literal character.
    /// </summary>
    private void ParseNonLiteralCharacter()
    {
        char ch = RightCharMoveRight();
        switch (ch)
        {
            // Start of a group
            case '(':
                StartGroup();
                break;

            // End of a group
            case ')':
                CloseGroup();
                _previousWasQuantifier = false;
                break;

            // Character class [...]
            case '[':
                AddNode(ParseCharacterClass());
                _previousWasQuantifier = false;
                break;

            // Escaped character
            case '\\':
                AddNode(ParseBackslash());
                _previousWasQuantifier = false;
                break;

            // StartOfLine anchor "^"
            case '^':
                AddNode(new StartOfLineNode());
                _previousWasQuantifier = false;
                break;

            // EndOfLine Anchor "$"
            case '$':
                AddNode(new EndOfLineNode());
                _previousWasQuantifier = false;
                break;

            // Any character (wildcard) "."
            case '.':
                AddNode(new AnyCharacterNode());
                _previousWasQuantifier = false;
                break;

            // End of an alternate
            case '|':
                AddAlternate();
                break;

            // Quantifier
            default:
                ParseQuantifier(ch);
                break;
        }
    }

    /// <summary>
    /// Parse a character class [...] in response to an opening '['.
    /// </summary>
    private CharacterClassNode ParseCharacterClass()
    {
        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.UnterminatedCharacterClass);
        }

        char ch;
        RegexNode previousNode;
        CharacterClassNode subtraction = null;
        bool negated = false;
        List<RegexNode> characterSet = [];

        if (RightChar() == '^')
        {
            MoveRight();
            negated = true;
        }

        previousNode = FirstCharacterClassElement();

        while (CharsRight() > 1 && (ch = RightChar()) != ']')
        {
            MoveRight();

            switch (ch)
            {
                // Could be range or subtraction.
                case '-':
                    previousNode = ParseCharacterClassDash(characterSet, previousNode, ref subtraction);
                    break;

                // Escape character, shorthand or unicode category.
                case '\\':
                    characterSet.Add(previousNode);
                    previousNode = ParseBasicBackslash();
                    break;

                // Literal character
                default:
                    characterSet.Add(previousNode);
                    previousNode = new CharacterNode(ch);
                    break;
            }
        }

        characterSet.Add(previousNode);

        if (RightChar() == ']')
        {
            MoveRight();
            CharacterClassCharacterSetNode characterSetNode = new(characterSet);

            return subtraction == null
                ? new CharacterClassNode(characterSetNode, negated)
                : new CharacterClassNode(characterSetNode, subtraction, negated);
        }

        // No closing ']'.
        throw MakeException(RegexParseError.UnterminatedCharacterClass);
    }

    /// <summary>
    /// Parse the first element (after optional negation '^') of a character class [...].
    /// A '-' character will always be parsed as a literal character at this position.
    /// </summary>
    private RegexNode FirstCharacterClassElement()
    {
        char ch;
        if (CharsRight() > 0 && (ch = RightCharMoveRight()) != ']')
        {
            // Escape character, shorthand or unicode category
            return ch == '\\' ? ParseBasicBackslash() : new CharacterNode(ch);
        }

        // Don't allow empty character class.
        else
        {
            throw MakeException(RegexParseError.UnterminatedCharacterClass);
        }
    }

    /// <summary>
    /// Parse characters following a dash '-' in a character class [...]. It could be part of a character range x-y or character class subtraction -[...]
    /// </summary>
    /// <param name="characterSet">The set of characters already in the characterclass.</param>
    /// <param name="previousNode">The character class element left of the dash '-'.</param>
    /// <param name="subtraction">A character class will be saved here if the dash '-' is part of a character class subtraction.</param>
    /// <returns></returns>
    private RegexNode ParseCharacterClassDash(List<RegexNode> characterSet, RegexNode previousNode, ref CharacterClassNode subtraction)
    {
        // '-' at the and of a character class is a literal character.
        if (RightChar() == ']')
        {
            characterSet.Add(previousNode);
            return new CharacterNode('-');
        }

        // Subtraction [...-[...]]
        else if (RightChar() == '[')
        {
            MoveRight();
            subtraction = ParseCharacterClassSubtraction();
            return previousNode;
        }

        // Could be range.
        else
        {
            // '-' after character class shorthand, unicode category/block or range is a literal character.
            if (previousNode is CharacterClassShorthandNode or UnicodeCategoryNode or CharacterClassRangeNode)
            {
                characterSet.Add(previousNode);
                return new CharacterNode('-');
            }

            // Range.
            else
            {
                return ParseCharacterRange(previousNode);
            }
        }
    }

    /// <summary>
    /// Parse a character class subtraction inside a character class.
    /// Throws an exception if the outer character class is not closed with a closing ']' or if the subtraction is not the last element in the outer character class.
    /// </summary>
    /// <returns></returns>
    private CharacterClassNode ParseCharacterClassSubtraction()
    {
        CharacterClassNode subtraction = ParseCharacterClass();

        // No closing ']' for outer character class.
        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.UnterminatedCharacterClass);
        }

        // Subtraction must be the last element in a character class.
        return RightChar() != ']' ? throw MakeException(RegexParseError.SubtractionMustBeLast) : subtraction;
    }

    /// <summary>
    /// Parse a character range [x-y] inside a character class.
    /// Throws an exception if the end of the range is a shorthand or unicode category or if start > end. 
    /// </summary>
    /// <param name="startNode">The start of the range. Should be a CharacterNode or EscapeCharacterNode.</param>
    private CharacterClassRangeNode ParseCharacterRange(RegexNode startNode)
    {
        RegexNode rangeEnd;
        char rangeEndChar;
        char ch = RightCharMoveRight();

        // End of range is an escape character
        if (ch == '\\')
        {
            rangeEnd = ParseBasicBackslash();

            // Don't allow shorthand or unicode category/block in a character range.
            if (rangeEnd is not EscapeCharacterNode escapeChar)
            {
                throw MakeException(RegexParseError.BadClassInCharacterRange, rangeEnd);
            }

            rangeEndChar = escapeChar.Character;
        }

        // End of range is a literal character
        else
        {
            rangeEnd = new CharacterNode(ch);
            rangeEndChar = ch;
        }

        char rangeStartChar = startNode is CharacterNode characterNode ? characterNode.Character : ((EscapeCharacterNode)startNode).Character;

        // The end of a character range should be greater than or equal to the start.
        if (rangeStartChar > rangeEndChar)
        {
            throw MakeException(RegexParseError.ReverseCharacterRange);
        }

        CharacterClassRangeNode range = new(startNode, rangeEnd);
        return range;
    }

    /// <summary>
    /// Adds a new GroupUnit to the group stack and sets it as the current group in response to an opening '('.
    /// </summary>
    private void StartGroup()
    {
        // Regular capturing group "(...)"
        if (IsCaptureGroup())
        {
            _group = new GroupUnit(new CaptureGroupNode());
        }

        // "(?...)" is a special group.
        else
        {
            MoveRight();
            char ch = RightCharMoveRight();

            switch (ch)
            {
                // Noncapturing group "(?:...)"
                case ':':
                    _group = new GroupUnit(new NonCaptureGroupNode());
                    break;

                // Atomic group "(?>...)"
                case '>':
                    _group = new GroupUnit(new AtomicGroupNode());
                    break;

                // Conditional group (?(condition)then|else)
                case '(':
                    MoveLeft();
                    _group = new GroupUnit(new ConditionalGroupNode());
                    break;

                // Possitive lookahead "(?=...)" or negative lookahead "(?!...)"
                case '=':
                case '!':
                    _group = new GroupUnit(new LookaroundGroupNode(true, ch == '='));
                    break;

                // Named capturing group "(?'name'...)" or balancing group "(?'name-balancedGroupName'...)"
                case '\'':
                    _group = new GroupUnit(CreateNamedGroup(ch));
                    break;


                // Named capturing group "(?<name>...)" or possitive lookbehind "(?<=...)" or negative lookbehind "(?<!...)"
                case '<':
                    StartNamedOrLookbehindGroup();
                    break;

                // Comment group "(?#...)" is not added to the group stack. It will be the prefix for the next node instead.
                case '#':
                    string comment = ScanComment();
                    _prefix = new CommentGroupNode(comment) { Prefix = _prefix };
                    // Return here, so nothing gets pushed to the group stack.
                    return;

                // Could be inline mode modifier group "(?imnsx-imnsx)" or "(?imnsx-imnsx:...)". Invalid grouping construct otherwise.
                default:
                    MoveLeft();
                    StartModeModifierGroup();
                    break;
            }
        }
        _groupStack.Push(_group);
    }

    /// <summary>
    /// Checks whether a group is a regular capturing group () in response to an opening '('.
    /// </summary>
    private bool IsCaptureGroup()
    {
        // "(" followed by nothing.
        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.NotEnoughParentheses);
        }

        // "(x..." where x != "?" or "(?)"
        return RightChar() != '?' || (CharsRight() > 1 && RightChar(1) == ')');
    }

    /// <summary>
    /// Sets a new GroupUnit with a ModeModifierGroupNode as the current group
    /// </summary>
    private void StartModeModifierGroup()
    {
        string options = ScanOptions();

        // Invalid grouping construct
        if (string.IsNullOrEmpty(options))
        {
            throw MakeException(RegexParseError.UnrecognizedGroupingConstruct);
        }

        // Mode modifier group "(?imnsx-imnsx)" or "(?imnsx-imnsx:...)"
        _group = new GroupUnit(new ModeModifierGroupNode(options));
    }

    private void StartNamedOrLookbehindGroup()
    {
        if (CharsRight() > 0)
        {
            char ch = RightChar();

            // Possitive lookbehind "(?<=...)" or negative lookbehind "(?<!...)"
            if (ch is '=' or '!')
            {
                // Consume both the '<' and the '=' or '!' characters.
                MoveRight();
                _group = new GroupUnit(new LookaroundGroupNode(false, ch == '='));
                return;
            }
        }

        // Named capturing group "(?<name>...)" or balancing group "(?<name-balancedGroupName>...)"
        _group = new GroupUnit(CreateNamedGroup('>'));
    }

    /// <summary>
    /// Creates a group unit for a named group.
    /// </summary>
    /// <param name="closeChar">The character that closes the named group. Should be '\'' or '>'.</param>
    private GroupNode CreateNamedGroup(char closeChar)
    {
        // Don't allow named group in condition of conditional group
        if (_group?.Node is ConditionalGroupNode && !_group.Node.ChildNodes.Any())
        {
            throw MakeException(RegexParseError.ConditionCantCapture);
        }

        bool useQuotes = closeChar == '\'';
        string groupName = ScanGroupName(closeChar, true);

        // Group name was closed. It is a named capturing group "(?<name>...)" or "(?'name'...)".
        if (LeftChar() == closeChar)
        {
            return new NamedGroupNode(groupName, useQuotes);
        }

        // Group name was not closed. It is a balancing group "(?<-balancedGroupName>...)" or "(?<name-balancedGroupName>...)".
        // Don't allow balancing a second time.
        string balancedGroupName = ScanGroupName(closeChar, false);

        // Unnamed balancing group "(?<-balancedGroupName>...)" or "(?'-balancedGroupName'...)"
        if (string.IsNullOrEmpty(groupName))
        {
            return new BalancingGroupNode(balancedGroupName, useQuotes);
        }

        // Named balancing group "(?<name-balancedGroupName>...)" or "(?'name-balancedGroupName'...)"
        return new BalancingGroupNode(balancedGroupName, groupName, useQuotes);
    }

    /// <summary>
    /// Remove a group from the top of the group stack and create a GroupNode from it in response to a closing ')'.
    /// </summary>
    private void CloseGroup()
    {
        if (_groupStack.Count == 0)
        {
            throw MakeException(RegexParseError.TooManyParentheses);
        }

        RegexNode outerNode = CreateOuterNode();
        GroupUnit currentGroup = _groupStack.Pop();
        RegexNode currentGroupNode = currentGroup.Node.AddNode(outerNode);
        _group = _groupStack.FirstOrDefault();


        // The first group "(...)" inside a conditional group goes directly to the condition group as it's condition.
        // TODO: "(...)" should be a reference node "(1)" without "\" or a named reference "(name)" without "?<>" if this is the name of a named group or a lookahead without "?=" otherwise.
        if (_group?.Node is ConditionalGroupNode && !_group.Node.ChildNodes.Any())
        {
            _group.Node = _group.Node.AddNode(currentGroupNode);
            return;
        }

        // No more than two alternates allowed in a conditional group
        if (_group?.Node is ConditionalGroupNode &&
            outerNode is AlternationNode &&
            outerNode.ChildNodes.Count() > MaxAlternatesInConditionalGroup)
        {
            throw MakeException(RegexParseError.TooManyAlternates);
        }

        AddNode(currentGroupNode);
    }

    /// <summary>
    /// Parse a quantifier alias '*', '+' or '?' or true quantifier "{n}", "{n,}" or "{n,m}".
    /// </summary>
    private void ParseQuantifier(char ch)
    {
        switch (ch)
        {
            // Quantifier alias '*', '+' or '?'
            case '*':
            case '+':
            case '?':
                ParseQuantifierAlias(ch);
                break;

            // Quantifier "{n}", "{n,}" or "{n,m}" or just a '{' character
            case '{':
                ParseTrueQuantifier();
                break;

            // Unrecognized character
            default:
                throw MakeException(RegexParseError.InternalError);
        }
    }

    /// <summary>
    /// Create a quantifier node in response to a '*', '+' or '?'. The quantifier node will replace the last concatenation item in the current group.
    /// Throws an exception if there are no concatenation items in the current group or if the previous node was a quantifier.
    /// </summary>
    private void ParseQuantifierAlias(char ch)
    {
        List<RegexNode> currentConcatenation = CurrentConcatenation();

        // Don't allow empty quantifiers
        if (!currentConcatenation.Any())
        {
            throw MakeException(RegexParseError.EmptyQuantifier);
        }

        // Don't allow nested quantifiers
        if (_previousWasQuantifier)
        {
            throw MakeException(RegexParseError.NestedQuantifier);
        }

        RegexNode previousNode = currentConcatenation.Last();

        QuantifierNode quantifier = ch switch
        {
            '*' => new QuantifierStarNode(previousNode),
            '+' => new QuantifierPlusNode(previousNode),
            '?' => new QuantifierQuestionMarkNode(previousNode),
            _ => throw MakeException(RegexParseError.InternalError)
        };

        // Quantifier followed by '?' is a lazy quantifier
        currentConcatenation[^1] = IsLazy() ? new LazyNode(quantifier) : quantifier;

        _previousWasQuantifier = true;
    }

    /// <summary>
    /// Parse a quantifier "{n}", "{n,}" or "{n,m}" in response to a '{'.
    /// The quantifier node will replace the last concatenation item in the current group.
    /// Throws an exception if there are no concatenation items in the current group or if the previous node was a quantifier.
    /// If the characters following the opening '{' don't follow this format, the '{' is a literal character instead.
    /// </summary>
    private void ParseTrueQuantifier()
    {
        int startPosition = _currentPosition;
        string n = ScanDecimals();
        QuantifierNode quantifier = null;
        char ch;

        // No decimal number after opening '{' or decimal number followed by nothing. '{' is a literal character.
        if (string.IsNullOrEmpty(n) || CharsRight() == 0)
        {
            AddNode(new CharacterNode('{'));
            return;
        }

        // Opening '{', followed by some decimal number, followed by closing '}'. It is a quantifier "{n}".
        if ((ch = RightCharMoveRight()) == '}')
        {
            RegexNode previousNode = CurrentConcatenation().LastOrDefault();
            quantifier = new QuantifierNNode(n, previousNode);
        }

        // Opening '{', followed by some decimal number, followed by ','. Could be "{n,}" or "{n,m}".
        else if (ch == ',')
        {
            string m = ScanDecimals();

            if (CharsRight() > 0 && RightCharMoveRight() == '}')
            {
                RegexNode previousNode = CurrentConcatenation().LastOrDefault();

                // Opening '{', followed by some decimal number, followed by ',', followed by closing '}'. It is a quantifier "{n,}".
                quantifier = string.IsNullOrEmpty(m)
                    ? new QuantifierNOrMoreNode(n, previousNode)
                    : int.Parse(n) <= int.Parse(m)
                        ? (QuantifierNode)new QuantifierNMNode(n, m, previousNode)
                        : throw MakeException(RegexParseError.IllegalQuantifierRange);
            }

        }

        // Characters following '{' followed format "{n}", "{n,}" or "{n,m}". It is a quantifier.
        if (quantifier != null)
        {
            AddQuantifier(quantifier);
        }

        // Characters following '{' did not follow format "{n}", "{n,}" or "{n,m}". '{' is a literal character.
        else
        {
            MoveTo(startPosition);
            AddNode(new CharacterNode('{'));
        }
    }

    /// <summary>
    /// Add a quantifier to the current concatenation items.
    /// The last element of the current concatenation items will become the quantifier's child en be replaced as the last element by the quantifier.
    /// </summary>
    private void AddQuantifier(QuantifierNode quantifier)
    {
        List<RegexNode> currentConcatenation = CurrentConcatenation();

        // Don't allow empty quantifiers
        if (!currentConcatenation.Any())
        {
            throw MakeException(RegexParseError.EmptyQuantifier);
        }

        // Don't allow nested quantifiers
        if (_previousWasQuantifier)
        {
            throw MakeException(RegexParseError.NestedQuantifier);
        }

        // Quantifier followed by '?' is a lazy quantifier
        currentConcatenation[^1] = IsLazy() ? new LazyNode(quantifier) : quantifier;

        _previousWasQuantifier = true;
    }

    /// <summary>
    /// Checks whether a quantifier is a lazy quantifier followed by a '?'.
    /// </summary>
    private bool IsLazy()
    {
        if (CharsRight() > 0 && RightChar() == '?')
        {
            MoveRight();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse characters following a '\'.
    /// </summary>
    /// <returns>BackreferenceNode, NamedRefrenceNode, AnchorNode, CharacterClassShorthandNode, UnicodeCategoryNode or EscapeNode</returns>
    private RegexNode ParseBackslash()
    {
        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.IllegalEndEscape);
        }

        char ch = RightChar();

        // Parse backreference "\1"
        // TODO: parse octal escape character if the digits following the '\' don't correspond to the number of a capturing group or named group.
        if (ch is >= '1' and <= '9')
        {
            string groupNumber = ScanDecimals();
            return new BackreferenceNode(int.Parse(groupNumber));
        }

        switch (ch)
        {
            // Named reference "\k<name>" or "\k'name'"
            case 'k':
                MoveRight();
                return ParseNamedReferenceNode(true);

            // Named references "\<name>" and "\'name'" are deprecated, but can still be used
            case '<':
            case '\'':
                return ParseNamedReferenceNode(false);

            // Anchor or character escape
            default:
                // Anchor
                if (IsAnchorCode(ch))
                {
                    MoveRight();
                    return AnchorNode.FromChar(ch);
                }

                // Character escape
                return ParseBasicBackslash();
        }
    }

    /// <summary>
    /// Parse and return a NamedReferenceNode in response to "\k", "\<" pr "\'".
    /// </summary>
    /// <param name="useK">Whether the named reference is openened with 'k'.</param>
    private NamedReferenceNode ParseNamedReferenceNode(bool useK)
    {
        const int minCharsInReference = 3;

        if (CharsRight() < minCharsInReference)
        {
            throw MakeException(RegexParseError.MalformedNamedReference);
        }

        bool useQuotes = RightCharMoveRight() == '\'';
        char closeChar = useQuotes ? '\'' : '>';
        return new NamedReferenceNode(ScanGroupName(closeChar, false), useQuotes, useK);
    }

    /// <summary>
    /// Parse characters following a '\' when it is not a backreference or an anchor.
    /// The escaped character can be used in a character class.
    /// </summary>
    /// <returns>CharacterClassShorthandNode, UnicodeCategoryNode or EscapeNode</returns>
    private RegexNode ParseBasicBackslash()
    {
        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.IllegalEndEscape);
        }

        char ch = RightChar();

        switch (ch)
        {
            // Character class shorthands
            case 'w':
            case 'W':
            case 's':
            case 'S':
            case 'd':
            case 'D':
                MoveRight();
                return new CharacterClassShorthandNode(ch);

            // Unicode category/block
            case 'p':
            case 'P':
                MoveRight();
                return ParseUnicodeCategoryNode(ch == 'P');

            // Character escape
            default:
                return ParseCharacterEscape();
        }
    }

    /// <summary>
    /// Parse and return a UnicodeCategryNode.
    /// </summary>
    /// <param name="negated">Whether the unicode category is negated "\P{X}" or not "\p{X}".</param>
    private UnicodeCategoryNode ParseUnicodeCategoryNode(bool negated)
    {
        const int minCharsInUnicodeCategory = 3;

        if (CharsRight() < minCharsInUnicodeCategory || RightCharMoveRight() != '{')
        {
            throw MakeException(RegexParseError.IncompleteUnicodeCategory);
        }

        int startPosition = _currentPosition;

        while (CharsRight() > 0 && RightChar() != '}')
        {
            MoveRight();
        }

        if (CharsRight() == 0)
        {
            throw MakeException(RegexParseError.IncompleteUnicodeCategory);
        }

        string categoryName = Pattern[startPosition.._currentPosition];
        MoveRight();
        return new UnicodeCategoryNode(categoryName, negated);
    }

    /// <summary>
    /// Parse an escaped characters following a '\'. It is an octal, hexadecimal, unicode or character escape.
    /// The escaped character can be used in a character class.
    /// </summary>
    private RegexNode ParseCharacterEscape()
    {
        char ch = RightChar();

        if (IsOctalDigit(ch))
        {
            return EscapeCharacterNode.FromOctal(ScanOctal());
        }

        MoveRight();

        switch (ch)
        {
            // Hexadecimal character \x00
            case 'x':
                return EscapeCharacterNode.FromHex(ScanHex(HexadecimalEscapeDigits));

            // Unicode character \u0000
            case 'u':
                return EscapeCharacterNode.FromUnicode(ScanHex(UnicodeEscapeDigits));

            // Control character \cA
            case 'c':
                return EscapeCharacterNode.FromControlCharacter(ScanControlCharacter());

            default:
                // Character escape
                if (IsEscapable(ch))
                {
                    return EscapeCharacterNode.FromCharacter(ch);
                }

                // Unescapable character
                throw MakeException(RegexParseError.UnrecognizedEscape, ch);
        }
    }

    /// <summary>
    /// Scans a character following \c. Valid control characters are A-Z and a-z.
    /// </summary>
    private char ScanControlCharacter()
    {
        if (CharsRight() > 0)
        {
            char ch = RightCharMoveRight();
            return ch is (>= 'A' and <= 'Z') or
                (>= 'a' and <= 'z') ? ch
                : throw MakeException(RegexParseError.UnrecognizedControl, ch);
        }
        throw MakeException(RegexParseError.MissingControl);
    }

    /// <summary>
    /// Scans decimal digits starting at the current parsing position until some other character is hit or end of the string.
    /// </summary>
    /// <returns>A string of decimal digits</returns>
    private string ScanDecimals()
    {
        int startPosition = _currentPosition;

        while (CharsRight() > 0 && IsDecimalDigit(RightChar()))
        {
            MoveRight();
        }

        return Pattern[startPosition.._currentPosition];
    }

    /// <summary>
    /// Scans c hexadecimal digits starting at the current parsing position.
    /// Use c = 2 for hexadecimal escapes "\x00".
    /// Use c = 4 for unicode escapes "\u0000".
    /// </summary>
    /// <param name="c">Number of characters to scan.</param>
    /// <returns>C hexadecimal digits</returns>
    private string ScanHex(int c)
    {
        if (CharsRight() >= c)
        {
            int startPosition = _currentPosition;

            for (; c > 0; c--)
            {
                char ch = RightCharMoveRight();
                if (!IsHexadecimalDigit(ch))
                {
                    throw MakeException(RegexParseError.NotEnoughHex);
                }
            }
            return Pattern[startPosition.._currentPosition];
        }

        throw MakeException(RegexParseError.NotEnoughHex);
    }

    /// <summary>
    /// Scans a maximum of three octal digits starting at the current parsing position.
    /// </summary>
    /// <returns>A string of octal digits</returns>
    private string ScanOctal()
    {
        int startPosition = _currentPosition;

        for (int c = 0; c < MaxDigitsInOctalEscape && CharsRight() > 0 && IsOctalDigit(RightChar()); c++)
        {
            MoveRight();
        }

        return Pattern[startPosition.._currentPosition];
    }

    /// <summary>
    /// Scan a group name in response to a '<' or '\''.
    /// </summary>
    /// <param name="closeChar">Character that closes the group name. Should be '>' or '\''.</param>
    /// <param name="allowBalancing">Allow a '-' to close the group name in case of a balancing group.</param>
    /// <returns>Group name</returns>
    private string ScanGroupName(char closeChar, bool allowBalancing)
    {
        const int minCharsToClosName = 2;

        if (CharsRight() < minCharsToClosName)
        {
            throw MakeException(RegexParseError.InvalidGroupName);
        }

        int startPosition = _currentPosition;
        char ch = RightChar();

        while (CharsRight() > 0 && IsWordChar(ch))
        {
            MoveRight();
            ch = RightChar();
        }

        if (ch == closeChar)
        {
            string groupName = Pattern[startPosition.._currentPosition];
            MoveRight();
            return groupName;
        }

        // Balancing group "(?<name-balancedGroupName>)" or "(?<-balancedGroupName>)"
        // Don't allow balancing a second time
        if (ch == '-' && allowBalancing)
        {
            string groupName = Pattern[startPosition.._currentPosition];
            MoveRight();
            return groupName;
        }

        throw MakeException(RegexParseError.InvalidGroupName);
    }

    /// <summary>
    /// Scans a group for inline mode modifiers. Only "imnsx-+" characters can be used for the options part of a ModeModifierGroup.
    /// </summary>
    /// <returns>Options as a string.</returns>
    private string ScanOptions()
    {
        // Mode modifiers in the condition of a conditional group are not allowed.
        if (_group?.Node is ConditionalGroupNode)
        {
            throw MakeException(RegexParseError.UnrecognizedGroupingConstruct);
        }

        int startPosition = _currentPosition;

        while (CharsRight() > 0 && IsValidOption(RightChar()))
        {
            MoveRight();
        }

        if (CharsRight() > 0)
        {
            char ch = RightChar();

            if (ch == ':')
            {
                string options = Pattern[startPosition.._currentPosition];
                MoveRight();
                return options;
            }

            return ch == ')' ? Pattern[startPosition.._currentPosition] : throw MakeException(RegexParseError.UnrecognizedGroupingConstruct);
        }

        throw MakeException(RegexParseError.UnrecognizedGroupingConstruct);
    }

    /// <summary>
    /// Scans a comment until a closing ')' is found.
    /// </summary>
    /// <returns>The comment.</returns>
    private string ScanComment()
    {
        int startPosition = _currentPosition;

        while (CharsRight() > 0 && RightChar() != ')')
        {
            MoveRight();
        }

        if (CharsRight() > 0)
        {
            string comment = Pattern[startPosition.._currentPosition];
            MoveRight();
            return comment;
        }

        throw MakeException(RegexParseError.UnterminatedCommentGroup);
    }

    /// <summary>
    /// Checks whether a character is a valid \ anchor code.
    /// </summary>
    private bool IsAnchorCode(char ch)
    {
        return ch switch
        {
            'A' or 'Z' or 'z' or 'G' or 'b' or 'B' => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks whether a character is escapable. Escapable characters are 'a', 'b', 'e', 'f', 'n', 'r', 't', 'v' and non-word characters.
    /// </summary>
    private bool IsEscapable(char ch)
    {
        switch (ch)
        {
            // Character escape
            case 'a':
            case 'b':
            case 'e':
            case 'f':
            case 'n':
            case 'r':
            case 't':
            case 'v':
                return true;

            default:
                // Other word characters should not be escaped.
                if (IsWordChar(ch))
                {
                    return false;
                }

                // Escape metacharacter
                return true;
        }
    }

    /// <summary>
    /// Checks whether a character is a valid inline option. Valid options are 'i', 'm', 'n', 's', 'x', '-', '+'.
    /// </summary>
    private bool IsValidOption(char ch)
    {
        return ch switch
        {
            'i' or 'm' or 'n' or 's' or 'x' or '-' or '+' => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks whether a character is a decimal digit 0-9
    /// </summary>
    private bool IsDecimalDigit(char ch)
    {
        return ch is >= '0' and <= '9';
    }

    /// <summary>
    /// Checks whether a character is a hexadecimal digit 0-9 || A-F || a-f
    /// </summary>
    private bool IsHexadecimalDigit(char ch)
    {
        ch = char.ToLower(ch);
        return IsDecimalDigit(ch) ||
            (ch >= 'a' && ch <= 'f');
    }

    /// <summary>
    /// Checks whether a character is an octal digit 0-7
    /// </summary>
    private bool IsOctalDigit(char ch)
    {
        return ch is >= '0' and <= '7';
    }

    /// <summary>
    /// Checks whether a character is a word character.
    /// A character is considered a word character if it belongs to one of the following unicode categories:
    /// UppercaseLetter (Lu), LowercaseLetter (Ll), TitlecaseLetter (Lt), ModifierLetter (Lm), OtherLetter (Lo),
    /// NonSpacingMark (Mn), SpacingCombiningMark (Mc), DecimalDigitNumber (Nd), ConnectorPunctuation (Pc)
    /// </summary>
    private bool IsWordChar(char ch)
    {
        return _wordCharCategories.Contains(char.GetUnicodeCategory(ch));
    }

    /// <summary>
    /// Create an outer node for the current group.
    /// The outer node will be an AlternationNode if the current group has alternates.
    /// Otherwise the outer node will be a ConcatenationNode if the current group has concatenation items.
    /// Otherwise the outer node will be a EmptyNode. Adds a prefix to the empty node if there is one and resets _prefix to null;
    /// </summary>
    private RegexNode CreateOuterNode()
    {

        if (CurrentAlternates().Any())
        {
            AddAlternate();
            return CreateAlternationNode();
        }

        if (CurrentConcatenation().Any())
        {
            return CreateConcatenationNode();
        }

        EmptyNode emptyNode = new() { Prefix = _prefix };
        _prefix = null;
        return emptyNode;
    }

    /// <summary>
    /// Creates a ConcatenationNode from the concatenation items of the current group or the outer concatenation.
    /// </summary>
    private ConcatenationNode CreateConcatenationNode()
    {
        List<RegexNode> currentConcatenation = CurrentConcatenation();

        // If there is a prefix left, add an empty node to hold the prefix.
        if (_prefix != null)
        {
            EmptyNode emptyNode = new() { Prefix = _prefix };
            _prefix = null;
            currentConcatenation.Add(emptyNode);
        }

        ConcatenationNode concatenationNode = new(currentConcatenation);
        currentConcatenation.Clear();
        return concatenationNode;
    }

    /// <summary>
    /// Creates an AlternationNode from the current group's alternates.
    /// </summary>
    private AlternationNode CreateAlternationNode()
    {
        List<RegexNode> currentAlternetes = CurrentAlternates();
        AlternationNode alternationNode = new(currentAlternetes);
        currentAlternetes.Clear();
        return alternationNode;
    }

    /// <summary>
    /// Adds a ConcatenationNode from the current group's concatenation items to it's alternates.
    /// Adds an EmptyNode if there are no concatenation items.
    /// Adds an EmptyNode to the last alternate to hold a prefix if there is one.
    /// </summary>
    private void AddAlternate()
    {
        if (CurrentConcatenation().Any())
        {
            // Add an EmptyNode to hold a prefix if there is one.
            if (_prefix != null)
            {
                EmptyNode emptyNode = new() { Prefix = _prefix };
                _prefix = null;
                CurrentConcatenation().Add(emptyNode);
            }

            CurrentAlternates().Add(CreateConcatenationNode());
        }

        else
        {
            EmptyNode emptyNode = new() { Prefix = _prefix };
            _prefix = null;
            CurrentAlternates().Add(emptyNode);
        }
    }

    /// <summary>
    /// Adds a RegexNode to the current group's concatenation items. Adds a prefix to the node if there is one and resets the prefix to null.
    /// </summary>
    /// <param name="node"></param>
    private void AddNode(RegexNode node)
    {
        node.Prefix = _prefix;
        _prefix = null;
        CurrentConcatenation().Add(node);
    }

    /// <summary>
    /// Checks whether a character is one of the metacharacters in ".\$^|()*+?{[".
    /// </summary>
    private static bool IsSpecial(char ch)
    {
        return @".\$^|()*+?{[".Contains(ch);
    }

    /// <summary>
    /// Return the concatenation items of the current group
    /// </summary>
    /// <returns></returns>
    private List<RegexNode> CurrentConcatenation()
    {
        return _group?.Concatenation ?? _concatenation;
    }

    /// <summary>
    /// Return the alternates of the current group
    /// </summary>
    private List<RegexNode> CurrentAlternates()
    {
        return _group?.Alternates ?? _alternates;
    }

    /// <summary>
    /// Creates a RegexParseException based on the type of RegexParseError.
    /// </summary>
    /// <param name="error">The error that caused the exception.</param>
    /// <param name="faults">The faulty token(s) that caused the error.</param>
    private RegexParseException MakeException(RegexParseError error, params object[] faults)
    {
        string errorDescription = string.Format(error.GetDescription(), faults);
        string message = $"Invalid pattern \"{Pattern}\" at offset {_currentPosition}. {errorDescription}";
        return new RegexParseException(error, _currentPosition, message);
    }

    /// <summary>
    /// Returns the number of characters to the right of the current parsing position.
    /// </summary>
    private int CharsRight()
    {
        return Pattern.Length - _currentPosition;
    }

    /// <summary>
    /// Moves the current parsing position one to the right.
    /// </summary>
    private void MoveRight()
    {
        _currentPosition++;
    }

    /// <summary>
    /// Moves the current parsing position one to the right.
    /// </summary>
    private void MoveLeft()
    {
        _currentPosition--;
    }

    /// <summary>
    /// Moves the current parsing position to i.
    /// </summary>
    private void MoveTo(int i)
    {
        _currentPosition = i;
    }

    /// <summary>
    /// Returns the character right of the current parsing position.
    /// </summary>
    private char RightChar()
    {
        return Pattern[_currentPosition];
    }

    /// <summary>
    /// Returns the character i characters right of the current parsing position.
    /// </summary>
    private char RightChar(int i)
    {
        return Pattern[_currentPosition + i];
    }

    /// <summary>
    /// Returns the character left of the current parsing position.
    /// </summary>
    private char LeftChar()
    {
        return Pattern[_currentPosition - 1];
    }

    /// <summary>
    /// Returns the character to the right of the current parsing position and moves the current parsing position one the the right.
    /// </summary>
    private char RightCharMoveRight()
    {
        return Pattern[_currentPosition++];
    }
}
