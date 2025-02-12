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

    private static void Execute(
        SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<GenerationData> Data) touple)
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
            indentWriter.WriteLine("#pragma warning disable CA5394");

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

            var builderProperties = data.Node.Members
                .OfType<PropertyDeclarationSyntax>()
                .ToArray();

            var builderMethods = data.Node.Members
                .OfType<MethodDeclarationSyntax>()
                .ToArray();

            var properties = data.TargetType
                .GetMembers()
                .OfType<IPropertySymbol>();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyType = property.Type;

                var variableName = propertyName.ToCamelCase();
                var fieldName = $"_{variableName}";

#pragma warning disable S125

                // EXPERIMENTAL
                // indentWriter.WriteLine($"public partial {fixedPropertyType} Default{propertyName} {{ get; }}");
#pragma warning restore S125

                var defaultPropertyName = $"Default{data.TargetType.MetadataName}{propertyName}";
                var hasAlreadyDefinedDefaultProperty = builderProperties.Any(x => x.Identifier.ValueText == defaultPropertyName);
                if (!hasAlreadyDefinedDefaultProperty)
                {
                    indentWriter.Write($"public static {propertyType} {defaultPropertyName} {{ get; }} = ");
                    switch (propertyType.Name)
                    {
                        case nameof(Guid):
                            indentWriter.WriteLine("global::System.Guid.NewGuid();");
                            break;
                        case nameof(Int32):
                            indentWriter.WriteLine("global::System.Random.Shared.Next();");
                            break;
                        case nameof(Int64):
                            indentWriter.WriteLine("global::System.Random.Shared.NextInt64();");
                            break;
                        case nameof(Single):
                            indentWriter.WriteLine("global::System.Random.Shared.NextSingle();");
                            break;
                        case nameof(Double):
                            indentWriter.WriteLine("global::System.Random.Shared.NextDouble();");
                            break;
                        case "string":
                        case nameof(String) when propertyType.NullableAnnotation != NullableAnnotation.Annotated:
                            indentWriter.WriteLine($"\"{defaultPropertyName}\";");
                            break;
                        case nameof(DateTime):
                            indentWriter.WriteLine("global::System.DateTime.UtcNow;");
                            break;
                        case nameof(DateTimeOffset):
                            indentWriter.WriteLine("global::System.DateTimeOffset.UtcNow;");
                            break;
                        case "" when propertyType.Kind == SymbolKind.ArrayType:
                        case not "" when propertyType.ContainingNamespace.ToDisplayString().Equals("System.Collections.Generic", StringComparison.Ordinal):
                            indentWriter.WriteLine("[];");
                            break;
                        default:
                            indentWriter.WriteLine("default;");
                            break;
                    }
                }

                indentWriter.WriteLine($"private {propertyType} {fieldName} = {defaultPropertyName};");
                var methodName = $"With{propertyName}";
                var hasAlreadyDefinedMethod = builderMethods.Any(x => x.Identifier.ValueText == methodName);
                if (!hasAlreadyDefinedMethod)
                {
                    indentWriter.WriteLine(
                        $"public {builderIdentifier} {methodName}({propertyType} {variableName})");
                    indentWriter.WriteLine("{");
                    indentWriter.Indent++;

                    indentWriter.WriteLine($"{fieldName} = {variableName};");
                    indentWriter.WriteLine("return this;");

                    indentWriter.Indent--;
                    indentWriter.WriteLine("}");
                }

                indentWriter.WriteLineNoTabs(string.Empty);
            }

            indentWriter.WriteLine($"public static {builderIdentifier} Default => new {builderIdentifier}();");
            indentWriter.WriteLine($"public partial {data.TargetType.ToDisplayString()} Build();");

            indentWriter.Indent--;
            indentWriter.WriteLine("}");

            indentWriter.WriteLine();
            indentWriter.Write("// </auto-generated>");

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
