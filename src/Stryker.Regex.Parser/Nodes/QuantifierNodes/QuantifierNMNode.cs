using System.Linq;

namespace Stryker.Regex.Parser.Nodes.QuantifierNodes;

/// <summary>
/// RegexNode representing a quantifier "{n,m}" where m >= n.
/// </summary>
public class QuantifierNMNode : QuantifierNode
{
    public int N { get; }
    public int M { get; }
    // n and m can be written with leading zeroes as 005 in the regex
    public string OriginalN { get; }
    public string OriginalM { get; }

    private QuantifierNMNode(string n, string m)
    {
        OriginalN = n;
        N = int.Parse(n);
        OriginalM = m;
        M = int.Parse(m);
    }

    public QuantifierNMNode(string n, string m, RegexNode childNode)
        : base(childNode)
    {
        OriginalN = n;
        N = int.Parse(n);
        OriginalM = m;
        M = int.Parse(m);
    }

    public QuantifierNMNode(int n, int m, RegexNode childNode)
        : base(childNode)
    {
        N = n;
        M = m;
    }

    protected override RegexNode CopyInstance()
    {
        return new QuantifierNMNode(OriginalN ?? N.ToString(), OriginalM ?? M.ToString());
    }

    public override string ToString()
    {
        return $"{ChildNodes.FirstOrDefault()}{Prefix}{{{OriginalN ?? N.ToString()},{OriginalM ?? M.ToString()}}}";
    }
}
