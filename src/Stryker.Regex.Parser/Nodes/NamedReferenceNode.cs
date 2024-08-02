namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing a named reference "\k&lt;Name&gt;".
/// </summary>
public class NamedReferenceNode : RegexNode
{
    public string Name { get; }
    public bool UseQuotes { get; }
    // Named reference \<name> and \'name' withou \k is deprecated, but can still be used.
    public bool UseK { get; }

    public NamedReferenceNode(string name, bool useQuotes)
        : this(name, useQuotes, true)
    {
    }

    public NamedReferenceNode(string name, bool useQuotes, bool useK)
    {
        Name = name;
        UseQuotes = useQuotes;
        UseK = useK;
    }

    protected override RegexNode CopyInstance()
    {
        return new NamedReferenceNode(Name, UseQuotes);
    }

    public override string ToString()
    {
        return $@"{Prefix}\{(UseK ? "k" : "")}{(UseQuotes ? "'" : "<")}{Name}{(UseQuotes ? "'" : ">")}";
    }
}
