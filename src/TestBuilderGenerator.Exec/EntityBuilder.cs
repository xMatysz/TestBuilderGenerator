namespace TestBuilderGenerator.Exec;

#pragma warning disable SA1015
[TestBuilderGenOf<Entity>]
#pragma warning restore SA1015
public partial class EntityBuilder
{
    public partial Entity Build()
    {
        _ = _number;
        return new Entity
        {
            Id = _id,
            Number = _number,
            Text = _text
        };
    }
}
