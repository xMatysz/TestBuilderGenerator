using System.Collections.Generic;

namespace TestBuilderGenerator;

public class GenerationDataComparer : IEqualityComparer<GenerationData>
{
    public static readonly IEqualityComparer<GenerationData> Instance = new GenerationDataComparer();
    private GenerationDataComparer() { }

    public bool Equals(GenerationData x, GenerationData y) =>
        x!.Equals(y);

    public int GetHashCode(GenerationData obj) =>
        obj.GetHashCode();
}
