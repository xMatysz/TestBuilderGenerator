using Microsoft.CodeAnalysis;

namespace TestBuilderGenerator;

public record NamespaceInfo
{
    public bool IsGlobalNamespace { get; }
    public string FullName { get; }

    public NamespaceInfo(INamespaceSymbol namespaceSymbol)
    {
        IsGlobalNamespace = namespaceSymbol.IsGlobalNamespace;
        FullName = namespaceSymbol.ToDisplayString();
    }
}
