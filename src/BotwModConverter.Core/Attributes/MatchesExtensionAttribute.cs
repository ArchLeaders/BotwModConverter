namespace BotwModConverter.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MatchesExtensionAttribute(params string[] extensions) : Attribute
{
    public string[] Extensions { get; } = extensions;
}