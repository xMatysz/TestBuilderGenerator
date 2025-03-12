using System;
using Microsoft.CodeAnalysis;

namespace TestBuilderGenerator;

public record SymbolPropertyInfo : PropertyInfo
{
    public NullableAnnotation NullableAnnotation { get; }
    public bool IsCollectionType { get; }

    private SymbolPropertyInfo(
        string name,
        string type,
        NullableAnnotation nullableAnnotation,
        bool isCollectionType)
        : base(name, type)
    {
        NullableAnnotation = nullableAnnotation;
        IsCollectionType = isCollectionType;
    }

    public static SymbolPropertyInfo FromSymbol(IPropertySymbol symbol) =>
        new(
            symbol.Name,
            symbol.Type.ToDisplayString(),
            symbol.NullableAnnotation,
            IsCollection(symbol.Type));

    private static bool IsCollection(ITypeSymbol symbol) =>
        symbol.Kind == SymbolKind.ArrayType ||
        symbol.ContainingNamespace
            .ToDisplayString()
            .Equals("System.Collections.Generic", StringComparison.Ordinal);
}
