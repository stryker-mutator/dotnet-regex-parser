namespace Stryker.Regex.Parser.Nodes.GroupNodes;

/// <summary>
/// RegexNode representing an comment group "(?#...)".
/// The CommentGroupNode should only be used as a prefix for other RegexNodes.
/// CommentGroupNodes can be nested to represent multiple comment groups in a row.
/// An EmptyNode with a CommentGroup as prefix can be used to represent a comment group at the end of a regex.
/// </summary>
public class CommentGroupNode : GroupNode
{
    public string Comment { get; }

    public CommentGroupNode()
    {
    }

    public CommentGroupNode(string comment)
    {
        Comment = comment;
    }

    protected override int GetSpanStart()
    {
        return Parent == null ? 0 : Parent.GetSpan().Start - GetSpanLength();
    }

    protected override RegexNode CopyInstance()
    {
        return new CommentGroupNode(Comment);
    }

    public override string ToString()
    {
        return $"{Prefix}(?#{Comment})";
    }
}
