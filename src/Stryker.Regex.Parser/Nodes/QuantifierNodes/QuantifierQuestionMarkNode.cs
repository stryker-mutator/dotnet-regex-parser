using System.Linq;

namespace Stryker.Regex.Parser.Nodes.QuantifierNodes;

/// <summary>
/// RegexNode representing a quantifier "?".
/// </summary>
public class QuantifierQuestionMarkNode : QuantifierNode
{
    private QuantifierQuestionMarkNode()
    {
    }

    public QuantifierQuestionMarkNode(RegexNode childNode)
        : base(childNode)
    {
    }

    public override string ToString()
    {
        return $"{ChildNodes.FirstOrDefault()}{Prefix}?";
    }
}
