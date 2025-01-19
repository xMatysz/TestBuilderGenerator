using TestBuilderGenerator.Exec.Exec;

namespace TestBuilderGenerator.Exec.TestFold;

#pragma warning disable SA1015
[TestBuilderGenOf<Entt2>]
#pragma warning restore SA1015
public partial class Entt2Builder
{
    public partial Entt2 Build()
    {
        return new Entt2
        {
            Id = _id,
            Number = _number,
            Text = _text
        };
    }
}
