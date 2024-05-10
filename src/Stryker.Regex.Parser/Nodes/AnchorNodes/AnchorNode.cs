using System;

namespace Stryker.Regex.Parser.Nodes.AnchorNodes;

public abstract class AnchorNode : RegexNode
{
    /// <summary>
    /// Create an new instance of an AnchorNode based on a character folling a "\":
    /// 'A' creates a new StartOfStringNode instance.
    /// 'Z' creates a new EndOfStringZNode instance.
    /// 'z' creates a new EndOfStringNode instance.
    /// 'b' creates a new WordBoundaryNode instance.
    /// 'B' creates a new NonWordBoundaryNode instance.
    /// 'G' creates a new ContiguousMatchNode instance.
    /// </summary>
    internal static RegexNode FromChar(char ch)
    {
        return ch switch
        {
            'A' => new StartOfStringNode(),
            'Z' => new EndOfStringZNode(),
            'z' => new EndOfStringNode(),
            'b' => new WordBoundaryNode(),
            'B' => new NonWordBoundaryNode(),
            'G' => new ContiguousMatchNode(),
            _ => throw new ArgumentException($"Invalid code for AnchorNode: {ch}")
        };
    }
}
