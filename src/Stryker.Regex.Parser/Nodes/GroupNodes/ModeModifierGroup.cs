using System.Collections.Generic;
using System.Linq;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing a mode modifier group "(?imnsx-imnsx:...)".
/// </summary>
public class ModeModifierGroupNode : GroupNode
{
    private const int _childSpanOffset = 3;

    protected override int ChildSpanOffset => Modifiers.Length + _childSpanOffset;
    public string Modifiers { get; }

    public ModeModifierGroupNode(string modifiers)
    {
        Modifiers = modifiers;
    }

    public ModeModifierGroupNode(string modifiers, RegexNode childNode)
        : base(childNode)
    {
        Modifiers = modifiers;
    }

    public ModeModifierGroupNode(string modifiers, IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
        Modifiers = modifiers;
    }

    protected override RegexNode CopyInstance()
    {
        return new ModeModifierGroupNode(Modifiers);
    }

    public override string ToString()
    {
        return $"{Prefix}(?{Modifiers}{(ChildNodes.Any() ? $":{string.Concat(ChildNodes)}" : "")})";
    }
}
