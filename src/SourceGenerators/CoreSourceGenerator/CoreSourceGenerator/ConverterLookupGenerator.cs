using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CoreSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class ConverterLookupGenerator : IIncrementalGenerator
{
    internal const string MatchNameAttributeTypeName = "BotwModConverter.Core.Attributes.MatchesNameAttribute";
    internal const string MatchExtensionAttributeTypeName = "BotwModConverter.Core.Attributes.MatchesExtensionAttribute";
    internal const string MatchMagicAttributeTypeName = "BotwModConverter.Core.Attributes.MatchesMagicAttribute";
    internal const string I = "        ";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var matchesNameAttributedClassProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(MatchNameAttributeTypeName,
                predicate: (_, _) => true,
                transform: (n, _) => n.TargetNode
            );

        var matchesExtensionAttributedClassProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(MatchExtensionAttributeTypeName,
                predicate: (_, _) => true,
                transform: (n, _) => n.TargetNode
            );

        var matchesMagicAttributedClassProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(MatchMagicAttributeTypeName,
                predicate: (_, _) => true,
                transform: (n, _) => n.TargetNode
            );

        var compilation = context.CompilationProvider.Combine(
            matchesNameAttributedClassProvider
                .Collect()
                .Combine(matchesExtensionAttributedClassProvider.Collect())
                .Combine(matchesMagicAttributedClassProvider.Collect())
        );

        context.RegisterSourceOutput(compilation,
            (spc, source) => { GenerateCode(spc, source.Left, source.Right.Left.Left, source.Right.Left.Right, source.Right.Right); }
        );
    }

    private void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<SyntaxNode> matchesNameNodes, ImmutableArray<SyntaxNode> matchesExtensionNodes, ImmutableArray<SyntaxNode> matchesMagicNodes)
    {
        var matchesNameTypes = GetTypes(compilation, matchesNameNodes, MatchNameAttributeTypeName);
        var matchesNameCases = new StringBuilder();
        foreach (var (type, attribute) in matchesNameTypes) {
            var name = attribute.ConstructorArguments[0].Value;

            // lang=cs
            matchesNameCases.AppendLine($"""

                {I}if (name is "{name}")
                {I}     return new {type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat)}();
                """);
        }

        var matchesExtensionTypes = GetTypes(compilation, matchesExtensionNodes, MatchExtensionAttributeTypeName);
        var matchesExtensionCases = new StringBuilder();
        foreach (var (type, attribute) in matchesExtensionTypes) {
            var args = attribute.ConstructorArguments[0].Values.AsSpan();

            if (args.IsEmpty) continue;

            // lang=cs
            matchesExtensionCases.Append($"""

                {I}if (global::System.MemoryExtensions.EndsWith(name, "{args[0].Value}", global::System.StringComparison.InvariantCulture)
                """);

            foreach (var arg in args.Slice(1, args.Length - 1)) {
                matchesExtensionCases.Append($"""
                    
                    {I} || global::System.MemoryExtensions.EndsWith(name, "{arg.Value}", global::System.StringComparison.InvariantCulture)
                    """);
            }

            // lang=cs
            matchesExtensionCases.AppendLine(
                $"""
                )
                {I}     return new {type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat)}();
                """);
        }

        var matchesMagicTypes = GetTypes(compilation, matchesMagicNodes, MatchMagicAttributeTypeName);
        var matchesMagicCases = new StringBuilder();
        foreach (var (type, attribute) in matchesMagicTypes) {
            var args = attribute.ConstructorArguments[0].Values.AsSpan();

            if (args.IsEmpty) continue;

            // lang=cs
            matchesMagicCases.Append($"""

                {I}if (global::System.MemoryExtensions.StartsWith(data, "{args[0].Value}"u8)
                {I} || (data.Length > 0x11 && global::System.MemoryExtensions.StartsWith(data[0x11..], "{args[0].Value}"u8))
                """);

            foreach (var arg in args.Slice(1, args.Length - 1)) {
                // lang=cs
                matchesMagicCases.Append($"""
                    
                    {I} || global::System.MemoryExtensions.StartsWith(data, "{arg.Value}"u8)
                    {I} || (data.Length > 0x11 && global::System.MemoryExtensions.StartsWith(data[0x11..], "{arg.Value}"u8))
                    """);
            }

            // lang=cs
            matchesMagicCases.AppendLine(
                $"""
                )
                {I}     return new {type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat)}();
                """);
        }

        // lang=cs
        string code = $$"""
            // <auto-generated/>

            #nullable enable

            namespace BotwModConverter.Core.Utils;

            public static class ConverterLookup
            {
                public static global::BotwModConverter.Core.IConverter? Find(global::System.ReadOnlySpan<char> name)
                {{{matchesNameCases}}{{matchesExtensionCases}}
                    return null;
                }
                
                public static global::BotwModConverter.Core.IConverter? Find(global::System.ReadOnlySpan<byte> data)
                {{{matchesMagicCases}}
                    return null;
                }
            }
            """;

        context.AddSource("BotwModConverter.Core.ConverterLookup.g.cs", code);
    }

    private static IEnumerable<(INamedTypeSymbol Type, AttributeData Attribute)> GetTypes(Compilation compilation, ImmutableArray<SyntaxNode> nodes, string attributeTypeName)
    {
        ISymbol attribute = compilation.GetTypeByMetadataName(attributeTypeName)!;

        foreach (var node in nodes) {
            if (compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node) is INamedTypeSymbol type) {
                yield return (
                    Type: type,
                    Attribute: type.GetAttributes().First(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute))
                );
            }
        }
    }
}