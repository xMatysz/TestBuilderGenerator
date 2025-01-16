namespace TestBuilderGenerator.Exec;

#pragma warning disable SA1015
[TestBuilderGenOf<Entity>]
#pragma warning restore SA1015
public partial class EntityBuilder
{
    public partial Entity Build()
    {
        throw new NotImplementedException();
    }
}
