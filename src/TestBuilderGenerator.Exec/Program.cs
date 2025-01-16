using TestBuilderGenerator.Exec;

#pragma warning disable S1481
Entity ent = EntityBuilder.Default
    .WithId(Guid.Empty)
    .WithText("test")
    .WithNumber(1)
    .Build();
#pragma warning restore S1481
