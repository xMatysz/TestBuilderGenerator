using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TestBuilderGenerator;

public record BuilderInformation(
    NamespaceInfo Namespace,
    PropertyInfo[] Properties,
    MethodInfo[] Methods,
    string Name,
    SyntaxTokenList Modifiers)
{
    public NamespaceInfo Namespace { get; } = Namespace;
    public MethodInfo[] Methods { get; } = Methods;
    public string Name { get; } = Name;
    public PropertyInfo[] Properties { get; } = Properties;
    public SyntaxTokenList Modifiers { get; } = Modifiers;

    public virtual bool Equals(BuilderInformation other) =>
        Namespace.Equals(other!.Namespace) &&
        Properties.SequenceEqual(other.Properties) &&
        Name.Equals(other.Name, StringComparison.Ordinal) &&
        Modifiers.SequenceEqual(other.Modifiers) &&
        Methods.SequenceEqual(other.Methods);

    public override int GetHashCode() =>
        Namespace.GetHashCode() ^
        Properties.GetHashCode() ^
        Name.GetHashCode() ^
        Modifiers.GetHashCode() ^
        Methods.GetHashCode();
}
