using Stryker.Regex.Parser.Nodes.CharacterClass;
using Stryker.Regex.Parser.Nodes.GroupNodes;
using System;
using System.Collections.Generic;

namespace Stryker.Regex.Parser.Nodes;

public abstract class RegexNode
{
    private readonly List<RegexNode> _childNodes = [];
    private CommentGroupNode _prefix;

    public IEnumerable<RegexNode> ChildNodes => _childNodes;
    public RegexNode Parent { get; private set; }
    public CommentGroupNode Prefix
    {
        get => _prefix;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }

            _prefix = value;
        }
    }

    protected virtual int ChildSpanOffset { get; }

    protected RegexNode() { }

    protected RegexNode(RegexNode childNode)
    {
        Add(childNode);
    }

    protected RegexNode(IEnumerable<RegexNode> childNodes)
    {
        foreach (RegexNode childNode in childNodes)
        {
            Add(childNode);
        }
    }

    public IEnumerable<RegexNode> GetDescendantNodes()
    {
        foreach (RegexNode childNode in ChildNodes)
        {
            foreach (RegexNode descendant in childNode.GetDescendantNodes())
            {
                yield return descendant;
            }
            yield return childNode;
        }
    }

    /// <summary>
    /// Sets the current RegexNode as the parent of the new RegexNode and the new node to it's child nodes.
    /// </summary>
    /// <returns>The current RegexNode</returns>
    private void Add(RegexNode newNode)
    {
        newNode.Parent = this;
        _childNodes.Add(newNode);
    }

    /// <summary>
    /// Creates a deepcopy of the RegexNode.
    /// </summary>
    /// <param name="copyChildNodes">Whether child nodes should be copied as well. Defaults to false.</param>
    /// <returns>The current RegexNode</returns>
    internal RegexNode Copy(bool copyChildNodes = false)
    {
        RegexNode copy = CopyInstance();
        copy.Prefix = Prefix?.Copy() as CommentGroupNode;
        if (copyChildNodes)
        {
            copy.CopyChildNodes(_childNodes);
        }
        return copy;
    }

    /// <summary>
    /// Creates a new instance of the RegexNode's derived type, cast to a RegexNode, using the default constructor.
    /// Derived classes that contain field members or properties that should be copied should override this method to make sure the field members are copied.
    /// </summary>
    /// <returns>A new instance of the RegexNode's derived type, cast to a RegexNode</returns>
    protected virtual RegexNode CopyInstance()
    {
        Type type = GetType();
        return (RegexNode)Activator.CreateInstance(type, true);
    }

    /// <summary>
    /// Creates a deepcopy of each RegexNode in childNodes and adds it to the current RegexNode's childNodes.
    /// </summary>
    /// <param name="childNodes">childNodes to copy and add</param>
    /// <returns>The current RegexNode</returns>
    private void CopyChildNodes(IEnumerable<RegexNode> childNodes)
    {
        foreach (RegexNode childNode in childNodes)
        {
            RegexNode childCopy = childNode.Copy(true);
            Add(childCopy);
        }
    }

    /// <summary>
    /// Copies the RegexNode and it's child nodes and adds a new node to it's child nodes.
    /// </summary>
    /// <param name="newNode">The new node to add</param>
    /// <param name="returnRoot">Whether to create a copy of the tree from the root. Defaults to true.</param>
    /// <returns>Returns a modified copy of the tree's root if  returnRoot is set to true. Returns a modified copy of the current node if returnRoot is set to false.</returns>
    public RegexNode AddNode(RegexNode newNode, bool returnRoot = true)
    {
        RegexNode copy = Copy();

        foreach (RegexNode childNode in _childNodes)
        {
            copy.Add(childNode.Copy(true));
        }

        copy.Add(newNode);

        return returnRoot && Parent != null ? Parent.ReplaceNode(this, copy) : copy;
    }

    /// <summary>
    /// Copies the RegexNode and it's child nodes and replaces all instances of a RegexNode with another RegexNode.
    /// </summary>
    /// <param name="oldNode">The old node to replace</param>
    /// <param name="newNode">The new replacement node</param>
    /// <param name="returnRoot">Whether to create a copy of the tree from the root. Defaults to true.</param>
    /// <returns>Returns a modified copy of the tree's root if  returnRoot is set to true. Returns a modified copy of the current node if returnRoot is set to false.</returns>
    public RegexNode ReplaceNode(RegexNode oldNode, RegexNode newNode, bool returnRoot = true)
    {
        RegexNode copy = Copy();

        if (oldNode.Prefix != null)
        {
            newNode.AddPrefixToPrefix(oldNode.Prefix.Copy() as CommentGroupNode);
        }

        foreach (RegexNode childNode in _childNodes)
        {
            copy.Add(childNode == oldNode ? newNode : childNode.ReplaceNode(oldNode, newNode, false));
        }

        return returnRoot && Parent != null ? Parent.ReplaceNode(this, copy) : copy;
    }

    /// <summary>
    /// Copies the RegexNode and it's child nodes and removes all instances of a RegexNode.
    /// </summary>
    /// <param name="oldNode">The node to remove</param>
    /// <param name="returnRoot">Whether to create a copy of the tree from the root. Defaults to true.</param>
    /// <returns>Returns a modified copy of the tree's root if  returnRoot is set to true. Returns a modified copy of the current node if returnRoot is set to false.</returns>
    public RegexNode RemoveNode(RegexNode oldNode, bool returnRoot = true)
    {
        RegexNode copy = Copy();
        bool lastWasOldNode = false;

        foreach (RegexNode childNode in _childNodes)
        {
            if (childNode == oldNode)
            {
                lastWasOldNode = true;
            }

            else
            {
                RegexNode childCopy = childNode.RemoveNode(oldNode, false);
                if (lastWasOldNode && oldNode.Prefix != null)
                {
                    childCopy.AddPrefixToPrefix(oldNode.Prefix.Copy() as CommentGroupNode);
                }
                copy.Add(childCopy);
                lastWasOldNode = false;
            }

        }

        if (lastWasOldNode && oldNode.Prefix != null)
        {
            copy.Add(new EmptyNode { Prefix = oldNode.Prefix.Copy() as CommentGroupNode });
        }

        return returnRoot && Parent != null ? Parent.ReplaceNode(this, copy) : copy;
    }

    private void AddPrefixToPrefix(CommentGroupNode newPrefix)
    {
        RegexNode currentNode = this;
        while (currentNode.Prefix != null)
        {
            currentNode = currentNode.Prefix;
        }
        currentNode.Prefix = newPrefix;
    }

    /// <summary>
    /// Gets the span (int Start, int Length) of the current node. Prefixes are not included in the span.
    /// </summary>
    public (int Start, int Length) GetSpan()
    {
        return (GetSpanStart(), GetSpanLength());
    }

    protected virtual int GetSpanStart()
    {
        if (Parent == null)
        {
            return Prefix == null ? 0 : Prefix.ToString().Length;
        }

        int startPosition = Parent.GetSpan().Start + Parent.ChildSpanOffset;

        foreach (RegexNode siblingNode in Parent.ChildNodes)
        {
            if (siblingNode == this)
            {
                break;
            }

            startPosition += siblingNode.ToString().Length;

            if (Parent is AlternationNode or CharacterClassRangeNode or CharacterClassNode)
            {
                startPosition += 1;
            }
        }

        if (Prefix != null)
        {
            startPosition += Prefix.ToString().Length;
        }

        return startPosition;
    }

    protected virtual int GetSpanLength()
    {
        int length = ToString().Length;

        if (Prefix != null)
        {
            length -= Prefix.ToString().Length;
        }

        return length;
    }

    public abstract override string ToString();
}
