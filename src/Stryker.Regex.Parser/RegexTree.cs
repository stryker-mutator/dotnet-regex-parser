using Stryker.Regex.Parser.Nodes;

namespace Stryker.Regex.Parser;

/// <summary>
/// RegexTree is a wrapper for a RegexNode tree.
/// </summary>
public class RegexTree(RegexNode root)
{
    public RegexNode Root { get; } = root;
}
