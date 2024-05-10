namespace RegexParser
{
    public interface IParser
    {
        string Pattern { get; }

        RegexTree Parse();
    }
}
