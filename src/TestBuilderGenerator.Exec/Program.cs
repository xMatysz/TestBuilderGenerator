using TestBuilderGenerator.Exec;
using TestBuilderGenerator.Exec.TestFold;

#pragma warning disable S1481
Entity ent = EntityBuilder.Default
    .WithId(Guid.Empty)
    .WithText("test")
    .WithNumber(1)
    .Build();

var ent2 = EntityBuilder.Default.Build();
var enntt = Entt2Builder.Default.Build();
Console.WriteLine(Entt2Builder.DefaultNumber);
Console.WriteLine(Entt2Builder.DefaultId);
Console.ReadLine();
#pragma warning restore S1481
