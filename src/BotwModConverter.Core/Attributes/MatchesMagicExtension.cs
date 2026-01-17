using System.Collections.Immutable;

namespace BotwModConverter.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MatchesMagicAttribute(params string[] magic) : Attribute
{
    public string[] Magic { get; } = magic;
}