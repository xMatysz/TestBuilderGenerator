namespace TestBuilderGenerator;

public record GenerationData(BuilderInformation Builder, TargetClassInformation TargetClass)
{
    public BuilderInformation Builder { get; } = Builder;
    public TargetClassInformation TargetClass { get; } = TargetClass;

    public virtual bool Equals(GenerationData other) =>
        Builder.Equals(other!.Builder) &&
        TargetClass.Equals(other!.TargetClass);

    public override int GetHashCode() =>
        Builder.GetHashCode() ^ TargetClass.GetHashCode();
}
