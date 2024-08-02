using System.Linq;

namespace Stryker.Regex.Parser.Nodes.QuantifierNodes;

/// <summary>
/// RegexNode representing a quantifier "{n}".
/// </summary>
public class QuantifierNNode : QuantifierNode
{
    private QuantifierNNode(string n)
    {
        OriginalN = n;
        N = int.Parse(n);
    }

    public int N { get; }
    // n can be written with leading zeroes as 005 in the regex
    public string OriginalN { get; }

    public QuantifierNNode(string n, RegexNode childNode)
        : base(childNode)
    {
        OriginalN = n;
        N = int.Parse(n);
    }

    public QuantifierNNode(int n, RegexNode childNode)
        : base(childNode)
    {
        N = n;
    }

    protected override RegexNode CopyInstance()
    {
        return new QuantifierNNode(OriginalN ?? N.ToString());
    }

    public override string ToString()
    {
        return $"{ChildNodes.FirstOrDefault()}{Prefix}{{{OriginalN ?? N.ToString()}}}";
    }
}
