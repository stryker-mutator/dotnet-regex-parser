using System.Linq;

namespace Stryker.Regex.Parser.Nodes.QuantifierNodes;

/// <summary>
/// RegexNode representing a quantifier "{n,}".
/// </summary>
public class QuantifierNOrMoreNode : QuantifierNode
{
    private QuantifierNOrMoreNode(string n)
    {
        OriginalN = n;
        N = int.Parse(n);
    }

    public int N { get; }
    // n can be written with leading zeroes as 005 in the regex
    public string OriginalN { get; }

    public QuantifierNOrMoreNode(string n, RegexNode childNode)
        : base(childNode)
    {
        OriginalN = n;
        N = int.Parse(n);
    }

    public QuantifierNOrMoreNode(int n, RegexNode childNode)
        : base(childNode)
    {
        N = n;
    }

    protected override RegexNode CopyInstance()
    {
        return new QuantifierNOrMoreNode(OriginalN ?? N.ToString());
    }

    public override string ToString()
    {
        return $"{ChildNodes.FirstOrDefault()}{Prefix}{{{OriginalN ?? N.ToString()},}}";
    }
}
