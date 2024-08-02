using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing a named capture group "(?&lt;Name&gt;...)".
/// </summary>
public class NamedGroupNode : GroupNode
{
    private const int _childSpanOffset = 4;

    protected override int ChildSpanOffset => Name.Length + _childSpanOffset;
    public string Name { get; }
    public bool UseQuotes { get; }

    public NamedGroupNode(string name, bool useQuotes)
    {
        Name = name;
        UseQuotes = useQuotes;
    }

    public NamedGroupNode(string name, bool useQuotes, RegexNode childNode)
        : base(childNode)
    {
        Name = name;
        UseQuotes = useQuotes;
    }

    public NamedGroupNode(string name, bool useQuotes, IEnumerable<RegexNode> childNodes)
        : base(childNodes)
    {
        Name = name;
        UseQuotes = useQuotes;
    }

    protected override RegexNode CopyInstance()
    {
        return new NamedGroupNode(Name, UseQuotes);
    }

    public override string ToString()
    {
        return $"{Prefix}(?{(UseQuotes ? "'" : "<")}{Name}{(UseQuotes ? "'" : ">")}{string.Concat(ChildNodes)})";
    }
}
