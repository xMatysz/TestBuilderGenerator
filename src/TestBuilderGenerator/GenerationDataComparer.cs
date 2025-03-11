using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

internal sealed class GenerationDataComparer : IEqualityComparer<GenerationData>
{
    public bool Equals(GenerationData x, GenerationData y)
    {
        return IsSameNamespace(x!.ContainingNamespace, y!.ContainingNamespace) &&
               IsSameTarget(x.TargetType, y.TargetType) &&
               IsSameBuilder(x.Node, y.Node);
    }

    private static bool IsSameBuilder(ClassDeclarationSyntax x, ClassDeclarationSyntax y)
    {
        var isDifferentIdentifier = x.Identifier.ValueText != y.Identifier.ValueText;
        if (isDifferentIdentifier)
        {
            return false;
        }

        var propsX = x.Members.OfType<PropertyDeclarationSyntax>().ToArray();
        var propsY = y.Members.OfType<PropertyDeclarationSyntax>().ToArray();

        if (propsX.Length != propsY.Length)
        {
            return false;
        }

        var sameProperties = propsX.All(p =>
            propsY.Any(b => b.Identifier.ValueText == p.Identifier.ValueText &&
                            p.Type.ToFullString() == b.Type.ToFullString()));

        if (!sameProperties)
        {
            return false;
        }

        var methodsX = x.Members.OfType<MethodDeclarationSyntax>().ToArray();
        var methodsY = y.Members.OfType<MethodDeclarationSyntax>().ToArray();

        if (methodsX.Length != methodsY.Length)
        {
            return false;
        }

        return methodsX.All(p =>
            methodsY.Any(b => b.Identifier.ValueText == p.Identifier.ValueText &&
                              p.ReturnType.ToFullString() == b.ReturnType.ToFullString()));
    }

    private static bool IsSameTarget(ITypeSymbol x, ITypeSymbol y)
    {
        var isDifferentType = x.ToDisplayString() != y.ToDisplayString();
        if (isDifferentType)
        {
            return false;
        }

        var propsX = x.GetMembers().OfType<IPropertySymbol>().ToArray();
        var propsY = y.GetMembers().OfType<IPropertySymbol>().ToArray();

        if (propsX.Length != propsY.Length)
        {
            return false;
        }

        return propsX.All(p =>
            propsY.Any(b => b.Name == p.Name &&
                            p.Type.ToDisplayString() == b.Type.ToDisplayString()));
    }

    private static bool IsSameNamespace(INamespaceSymbol x, INamespaceSymbol y) =>
        x.IsGlobalNamespace == y.IsGlobalNamespace &&
        x.ToDisplayString() == y.ToDisplayString();

    private GenerationDataComparer() { }

    public static GenerationDataComparer Instance { get; } = new();

    public int GetHashCode(GenerationData obj) => 1;
}
