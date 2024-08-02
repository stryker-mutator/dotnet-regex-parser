using System.Linq;

namespace Stryker.Regex.Parser.Nodes.QuantifierNodes;

public abstract class QuantifierNode : RegexNode
{
    protected override int ChildSpanOffset => 0 - (ChildNodes.FirstOrDefault()?.ToString().Length ?? 0) - (Prefix?.ToString().Length ?? 0);

    protected QuantifierNode()
    {
    }

    protected QuantifierNode(RegexNode childNode)
        : base(childNode)
    {
    }

    protected override int GetSpanStart()
    {
        return base.GetSpanStart() + ChildNodes.FirstOrDefault()?.ToString().Length ?? 0;
    }

    protected override int GetSpanLength()
    {
        return base.GetSpanLength() - ChildNodes.FirstOrDefault()?.ToString().Length ?? 0;
    }
}
