namespace Stryker.Regex.Parser;

public interface IParser
{
    string Pattern { get; }

    RegexTree Parse();
}
