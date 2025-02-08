using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Text;

namespace TestBuilderGenerator.Tests;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
#pragma warning disable CA1000

    public static Task ValidateSourceGeneratorAsync(
        string[] sources,
        (string fileName, string content)[] generatedSources) =>
        ValidateSourceGeneratorAsync(sources, generatedSources, DiagnosticResult.EmptyDiagnosticResults);

    public static async Task ValidateSourceGeneratorAsync(
        string[] sources,
        (string fileName, string content)[] generatedSources,
        DiagnosticResult[] diagnostics)
    {
        var tester = new Test
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        foreach (var source in sources)
        {
            tester.TestState.Sources.Add(source);
        }

        foreach (var (fileName, content) in generatedSources)
        {
            tester.TestState.GeneratedSources.Add((typeof(TSourceGenerator), fileName,
                SourceText.From(content, Encoding.UTF8)));
        }

        tester.TestState.ExpectedDiagnostics.AddRange(diagnostics);

        await tester.RunAsync(CancellationToken.None);
    }
#pragma warning restore CA1000

    private sealed class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public static LanguageVersion LanguageVersion => LanguageVersion.Default;

        protected override ParseOptions CreateParseOptions() =>
            ((CSharpParseOptions)base.CreateParseOptions())
            .WithLanguageVersion(LanguageVersion);

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = ["/warnaserror:nullable"];

            var commandLineArguments = CSharpCommandLineParser.Default.Parse(
                args,
                baseDirectory: Environment.CurrentDirectory,
                sdkDirectory: Environment.CurrentDirectory);

            return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
        }
    }
}
