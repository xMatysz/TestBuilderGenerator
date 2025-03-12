using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

public record PropertyInfo
{
    public string Name { get; }
    public string Type { get; }

    protected PropertyInfo(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public static PropertyInfo FromDeclarationSyntax(PropertyDeclarationSyntax propertySyntax) =>
        new(propertySyntax.Identifier.ValueText, propertySyntax.Type.ToFullString());
}
