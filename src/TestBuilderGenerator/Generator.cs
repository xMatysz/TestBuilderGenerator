using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitializationCallback);

        // TODO: DONT UPDATE WHEN NO CHANGES
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            CodeTemplates.AttributeIdentifier,
            SyntaxProviderPredicate,
            SyntaxProviderTransform);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, (Compilation Compilation, ImmutableArray<GenerationData> Data) touple)
    {
        var sb = new StringBuilder();
        var writer = new StringWriter(sb);
        using var indentWriter = new IndentedTextWriter(writer);

        foreach (var data in touple.Data)
        {
            if (IsNotPartial(data.Node))
            {
                EmitErrorDiagnostic(context, data.Node);
                continue;
            }

            indentWriter.WriteLine("// <auto-generated/>");
            indentWriter.WriteLine("// lang=c#");
            indentWriter.WriteLine("#nullable enable");

            if (!data.Symbol.ContainingNamespace.IsGlobalNamespace)
            {
                // TODO: not file scoped namespaces
                indentWriter.WriteLine($"namespace {data.Symbol.ContainingNamespace.ToDisplayString()};");
                indentWriter.WriteLine();
            }

            var builderIdentifier = data.Node.Identifier;
            var accessibility = data.Node.Modifiers.First(x =>
                x.IsKind(SyntaxKind.PublicKeyword) ||
                x.IsKind(SyntaxKind.InternalKeyword));

            indentWriter.WriteLine($"{accessibility} partial class {builderIdentifier}");
            indentWriter.WriteLine("{");
            indentWriter.Indent++;

            var properties = data.TargetType.GetMembers().OfType<IPropertySymbol>();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyType = property.Type;

#pragma warning disable CA1308

                // TODO: camel case instead of lower case
                var variableName = propertyName.ToLowerInvariant();
#pragma warning restore CA1308
                var fieldName = $"_{variableName}";

                // TODO: make type resolver
                var defaultPropertyName = $"Default{propertyName}";
                switch (propertyType.Name)
                {
                    case nameof(Guid):
                        indentWriter.WriteLine($$"""public static {{propertyType}} {{defaultPropertyName}} { get; } = global::System.Guid.NewGuid();""");
                        break;
                    case nameof(Int32):
                        indentWriter.WriteLine("#pragma warning disable CA5394");
                        indentWriter.WriteLine($$"""public static {{propertyType}} {{defaultPropertyName}} { get; } = global::System.Random.Shared.Next();""");
                        indentWriter.WriteLine("#pragma warning restore CA5394");
                        break;
                    case nameof(Int64):
                        indentWriter.WriteLine("#pragma warning disable CA5394");
                        indentWriter.WriteLine($$"""public static {{propertyType}} {{defaultPropertyName}} { get; } = global::System.Random.Shared.NextInt64();""");
                        indentWriter.WriteLine("#pragma warning restore CA5394");
                        break;
                    case nameof(Single):
                        indentWriter.WriteLine("#pragma warning disable CA5394");
                        indentWriter.WriteLine($$"""public static {{propertyType}} {{defaultPropertyName}} { get; } = global::System.Random.Shared.NextSingle();""");
                        indentWriter.WriteLine("#pragma warning restore CA5394");
                        break;
                    case nameof(Double):
                        indentWriter.WriteLine("#pragma warning disable CA5394");
                        indentWriter.WriteLine($$"""public static {{propertyType}} {{defaultPropertyName}} { get; } = global::System.Random.Shared.NextDouble();""");
                        indentWriter.WriteLine("#pragma warning restore CA5394");
                        break;
                    case "string":
                    case nameof(String):
                        indentWriter.WriteLine($"public static {propertyType} {defaultPropertyName} => \"{defaultPropertyName}\";");
                        break;
                    default:
                        indentWriter.WriteLine(propertyType.IsReferenceType
                            ? $"public static {propertyType} {defaultPropertyName} => null;"
                            : $"public static {propertyType} {defaultPropertyName};");
                        break;
                }

                indentWriter.WriteLine($"private {propertyType} {fieldName} = {defaultPropertyName};");
                indentWriter.WriteLine($"public {builderIdentifier} With{propertyName}({propertyType} {variableName})");
                indentWriter.WriteLine("{");
                indentWriter.Indent++;

                indentWriter.WriteLine($"{fieldName} = {variableName};");
                indentWriter.WriteLine("return this;");

                indentWriter.Indent--;
                indentWriter.WriteLine("}");
                indentWriter.WriteLine();
            }

            indentWriter.WriteLine($"public static {builderIdentifier} Default => new {builderIdentifier}();");
            indentWriter.WriteLine($"public partial {data.TargetType.ToDisplayString()} Build();");
            indentWriter.WriteLine();

            indentWriter.Indent--;
            indentWriter.WriteLine("}");

            indentWriter.WriteLine();
            indentWriter.WriteLine("// </auto-generated>");

            context.AddSource($"{data.Node.Identifier}.g.cs", sb.ToString());
            sb.Clear();
        }
    }

    private static void EmitErrorDiagnostic(SourceProductionContext context, ClassDeclarationSyntax classDeclaration)
    {
        var identifier = classDeclaration.Identifier;
        var location = Location.Create(identifier.SyntaxTree!, identifier.FullSpan);
        var descriptor = new DiagnosticDescriptor(
            "SG0001",
            "Class is not partial",
            $"Missing partial modifier for class {identifier.ToFullString()}",
            "error",
            DiagnosticSeverity.Error,
            true);

        context.ReportDiagnostic(Diagnostic.Create(descriptor, location));
    }

    private static bool IsNotPartial(ClassDeclarationSyntax classDeclaration)
    {
        return !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static GenerationData SyntaxProviderTransform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var classDeclaration = context.TargetNode as ClassDeclarationSyntax;
        var targetType = context.Attributes.First().AttributeClass.TypeArguments.First();

        // CHECK IF MORE ATTRIBUTES
        return new GenerationData(classDeclaration, context.TargetSymbol, targetType);
    }

    private static bool SyntaxProviderPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax;
    }

    private static void PostInitializationCallback(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource($"{CodeTemplates.AttributeClassName}.g.cs", CodeTemplates.AttributeTemplate);
    }
}
