#nullable enable
#pragma warning disable CS8618
#pragma warning disable CA2227

namespace EntityNamespace;

public class Entity
{
    public int PredefinedStructProp { get; set; }
    public int? PredefinedStructPropNullable { get; set; }
    public string PredefinedRefProp { get; set; }
    public string? PredefinedRefPropNullable { get; set; }
    public int[] PredefinedRefArrayProp { get; set; }
    public int?[] PredefinedRefNullableArrayProp { get; set; }
    public IReadOnlyCollection<int> PredefinedRefListProp { get; set; }
}

#pragma warning restore CS8618
#pragma warning restore CA2227
