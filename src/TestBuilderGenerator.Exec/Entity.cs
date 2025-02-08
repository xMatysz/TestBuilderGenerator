#nullable enable
#pragma warning disable CS8618

namespace EntityNamespace;

public class Entity
{
    public int PredefinedStructProp { get; set; }
    public int? PredefinedStructPropNullable { get; set; }
    public string PredefinedRefProp { get; set; }
    public string? PredefinedRefPropNullable { get; set; }
}

#pragma warning restore CS8618
