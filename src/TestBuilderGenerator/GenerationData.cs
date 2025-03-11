using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

public record GenerationData(
    ClassDeclarationSyntax Node,
    INamespaceSymbol ContainingNamespace,
    ITypeSymbol TargetType)
{
    public ClassDeclarationSyntax Node { get; } = Node;
    public INamespaceSymbol ContainingNamespace { get; } = ContainingNamespace;
    public ITypeSymbol TargetType { get; } = TargetType;
}
