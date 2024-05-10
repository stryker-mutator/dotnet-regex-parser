using System.ComponentModel;
using System.Linq;

namespace Stryker.Regex.Parser.Exceptions;

public enum RegexParseError
{
    [Description("Internal error while parsing regex")]
    InternalError,
    [Description("Not enough closing parentheses ).")]
    NotEnoughParentheses,
    [Description("Too many closing parentheses ).")]
    TooManyParentheses,
    [Description("Unterminated character class [].")]
    UnterminatedCharacterClass,
    [Description("A subtraction must be the last element in a character class.")]
    SubtractionMustBeLast,
    [Description("Class {0} is not allowed in a character range.")]
    BadClassInCharacterRange,
    [Description("Character range [x-y] in reverse order.")]
    ReverseCharacterRange,
    [Description("Unrecognized grouping construct.")]
    UnrecognizedGroupingConstruct,
    [Description("Conditional group conditions do not capture and cannot be named.")]
    ConditionCantCapture,
    [Description("Too many alternates in conditional group (?()|)")]
    TooManyAlternates,
    [Description("Quantifier following nothing.")]
    EmptyQuantifier,
    [Description("Nested quantifier.")]
    NestedQuantifier,
    [Description("Illegal \\ at end of pattern.")]
    IllegalEndEscape,
    [Description("Malformed \\k<...> named backreference.")]
    MalformedNamedReference,
    [Description("Unrecognized escape sequence \\{0}.")]
    UnrecognizedEscape,
    [Description("Missing control character.")]
    MissingControl,
    [Description("Unrecognized control character \\c{0}.")]
    UnrecognizedControl,
    [Description("Not enough hexadecimal digits.")]
    NotEnoughHex,
    [Description("Invalid group name.")]
    InvalidGroupName,
    [Description("Incomplete \\p{{X}} unicode category or block.")]
    IncompleteUnicodeCategory,
    [Description("Unterminated comment group (?#... .")]
    UnterminatedCommentGroup,
    [Description("Illegal quantifier {x,y} with x > y.")]
    IllegalQuantifierRange,
}

internal static class RegexParseErrorExtension
{
    internal static string GetDescription(this RegexParseError error)
    {
        DescriptionAttribute descriptionAttribute = typeof(RegexParseError)
            .GetField(error.ToString())
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        return descriptionAttribute?.Description;
    }
}
