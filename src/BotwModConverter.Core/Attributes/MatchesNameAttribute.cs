namespace BotwModConverter.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MatchesNameAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}