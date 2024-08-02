using System;

namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing an escaped character "\a".
/// </summary>
public class EscapeCharacterNode : RegexNode
{
    private const int MaxOctalEscapeValue = 255;

    public string Escape { get; }
    public char Character { get; }

    private EscapeCharacterNode(string escape, char character)
    {
        Escape = escape;
        Character = character;
    }

    /// <summary>
    /// Create a new EscapeCharacterNode based on an escape character or escaped metacharacter.
    /// </summary>
    /// <param name="character">Character to escape</param>
    /// <returns>EscapeChararacterNode in format \a</returns>
    public static EscapeCharacterNode FromCharacter(char character)
    {
        char escapedChar = character switch
        {
            'a' => '\a',
            'b' => '\b',
            'f' => '\f',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            'v' => '\v',
            'e' => (char)27,
            _ => character
        };

        return new EscapeCharacterNode(character.ToString(), escapedChar);
    }

    /// <summary>
    /// Create a new EscapeCharacterNode based on a 2digit hexadecimal number.
    /// </summary>
    /// <param name="hex">2-digit hexadecimal number</param>
    /// <returns>EscapeChararacterNode in format \x00</returns>
    public static EscapeCharacterNode FromHex(string hex)
    {
        char character = (char)Convert.ToInt32(hex, 16);
        return new EscapeCharacterNode($"x{hex}", character);
    }

    /// <summary>
    /// Create a new EscapeCharacterNode based on a 4-digit hexadecimal number.
    /// </summary>
    /// <param name="unicode">4-digit hexadecimal number</param>
    /// <returns>EscapeChararacterNode in format \u0000</returns>
    public static EscapeCharacterNode FromUnicode(string unicode)
    {
        char character = (char)Convert.ToInt32(unicode, 16);
        return new EscapeCharacterNode($"u{unicode}", character);
    }

    /// <summary>
    /// Create a new EscapeCharacterNode based on a 2- or 3-digit octal number.
    /// </summary>
    /// <param name="octal">2- or 3-digit octal number</param>
    /// <returns>EscapeChararacterNode in format \000 or \00</returns>
    public static EscapeCharacterNode FromOctal(string octal)
    {
        int decimalValue = Convert.ToInt32(octal, 8);

        if (decimalValue > MaxOctalEscapeValue)
        {
            decimalValue ^= MaxOctalEscapeValue + 1;
        }

        char character = (char)decimalValue;
        return new EscapeCharacterNode(octal, character);
    }

    /// <summary>
    /// Create a new EscapeCharacterNode based on an ASCII control character a-z or A-Z.
    /// </summary>
    /// <param name="control">ASCII control character a-z or A-Z</param>
    /// <returns>EscapeChararacterNode in format \ca or \cA</returns>
    public static EscapeCharacterNode FromControlCharacter(char control)
    {
        char controlCharacter = (char)(char.ToUpper(control) - '@');
        return new EscapeCharacterNode($"c{control}", controlCharacter);
    }

    protected override RegexNode CopyInstance()
    {
        return new EscapeCharacterNode(Escape, Character);
    }

    public override string ToString()
    {
        return $@"{Prefix}\{Escape}";
    }
}
