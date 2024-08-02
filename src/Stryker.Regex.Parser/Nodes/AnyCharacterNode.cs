namespace Stryker.Regex.Parser.Nodes
{
    /// <summary>
    /// RegexNode representing the dot or wildcard token ".", matching any character.
    /// </summary>
    public class AnyCharacterNode : RegexNode
    {
        public override string ToString()
        {
            return $"{Prefix}.";
        }
    }
}
