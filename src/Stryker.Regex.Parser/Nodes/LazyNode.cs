using System.Linq;

namespace Stryker.Regex.Parser.Nodes;

/// <summary>
/// RegexNode representing a lazy token "?" used to make a quantifier lazy.
/// </summary>
public class LazyNode : RegexNode
{
    protected override int ChildSpanOffset => 0 - (ChildNodes.FirstOrDefault()?.ToString().Length ?? 0) - (Prefix?.ToString().Length ?? 0);

    private LazyNode()
    {
    }

    public LazyNode(RegexNode childNode)
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

    public override string ToString()
    {
        return $"{ChildNodes.FirstOrDefault()}{Prefix}?";
    }
}
