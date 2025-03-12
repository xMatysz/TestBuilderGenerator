using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

public record MethodInfo
{
    public string Name { get; }
    public string ReturnType { get; }

    private MethodInfo(string name, string returnType)
    {
        Name = name;
        ReturnType = returnType;
    }

    public static MethodInfo FromDeclarationSyntax(MethodDeclarationSyntax propertySyntax) =>
        new(propertySyntax.Identifier.ValueText, propertySyntax.ReturnType.ToFullString());

    public static MethodInfo FromSymbol(IMethodSymbol symbol)
        => new(symbol.ToDisplayString(), symbol.ReturnType.ToDisplayString());
}
