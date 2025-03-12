using System;
using System.Linq;

namespace TestBuilderGenerator;

public record TargetClassInformation(
    NamespaceInfo Namespace,
    SymbolPropertyInfo[] Properties,
    MethodInfo[] Methods,
    string Identity,
    string Name)
{
    public NamespaceInfo Namespace { get; } = Namespace;
    public MethodInfo[] Methods { get; } = Methods;
    public string Name { get; } = Name;
    public string Identity { get; } = Identity;
    public SymbolPropertyInfo[] Properties { get; } = Properties;

    public virtual bool Equals(TargetClassInformation other) =>
        Namespace.Equals(other!.Namespace) &&
        Properties.SequenceEqual(other.Properties) &&
        Name.Equals(other.Name, StringComparison.Ordinal) &&
        Identity.Equals(other.Identity, StringComparison.Ordinal) &&
        Methods.SequenceEqual(other.Methods);

    public override int GetHashCode() =>
        Namespace.GetHashCode() ^
        Name.GetHashCode() ^
        Identity.GetHashCode() ^
        Properties.GetHashCode() ^
        Methods.GetHashCode();
}
