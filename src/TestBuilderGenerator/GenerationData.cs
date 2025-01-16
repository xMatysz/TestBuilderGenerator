using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestBuilderGenerator;

public record GenerationData(
    ClassDeclarationSyntax Node,
    ISymbol Symbol,
    ITypeSymbol TargetType)
{
    public ClassDeclarationSyntax Node { get; } = Node;
    public ISymbol Symbol { get; } = Symbol;
    public ITypeSymbol TargetType { get; } = TargetType;
}
