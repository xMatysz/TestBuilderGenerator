namespace TestBuilderGenerator.Tests;

public class GeneratorTests
{
    private readonly (string fileName, string content) _attributeGeneratedCode = (
        "TestBuilderGenOfAttribute.g.cs",
        """
        // <auto-generated/>
        // lang=c#

        #nullable enable

        namespace TestBuilderGenerator;

        [global::System.CodeDom.Compiler.GeneratedCode("TestBuilderGenerator", "1.0.0.0")]
        [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
        internal sealed class TestBuilderGenOfAttribute<T> : global::System.Attribute
        {
        }

        // </auto-generated/>
        """);

    [Fact]
    public async Task Generate_Attribute()
    {
        await VerifyCS.ValidateSourceGeneratorAsync(
            [],
            [_attributeGeneratedCode]);
    }

    [Fact]
    public async Task Generate_Partial_Builder()
    {
        const string code1 =
            """
            namespace EntityNamespace;

            public class Entity
            {
            }
            """;

        const string code2 =
            """
            using EntityNamespace;
            using TestBuilderGenerator;

            namespace BuilderNamespace;

            [TestBuilderGenOf<Entity>]
            public partial class EntityBuilder
            {
                public partial Entity Build()
                {
                    throw new System.NotImplementedException();
                }
            }
            """;

        (string fileName, string content) generatedCode = (
            "EntityBuilder.g.cs",
            """
            // <auto-generated/>
            // lang=c#
            #nullable enable
            #pragma warning disable CA5394
            namespace BuilderNamespace;

            public partial class EntityBuilder
            {
                public static EntityBuilder Default => new EntityBuilder();
                public partial EntityNamespace.Entity Build();
            }

            // </auto-generated>
            """);

        await VerifyCS.ValidateSourceGeneratorAsync(
            [code1, code2],
            [_attributeGeneratedCode, generatedCode]);
    }

    [Theory]
    [InlineData("int", "global::System.Random.Shared.Next()")]
    [InlineData("int?", "default")]
    [InlineData("string", "\"DefaultEntityPropName\"")]
    [InlineData("string?", "default")]
    [InlineData("int[]", "[]")]
    [InlineData("int?[]", "[]")]
    [InlineData("System.Collections.Generic.List<int>", "[]")]
    [InlineData("System.Collections.Generic.IEnumerable<int>", "[]")]
    [InlineData("System.Collections.Generic.ICollection<int>", "[]")]
    [InlineData("System.Collections.Generic.IList<int>", "[]")]
    [InlineData("System.Collections.Generic.IReadOnlyCollection<int>", "[]")]
    public async Task Generate_Property_Setup(string propertyType, string defaultPropertyType)
    {
        var code1 =
            $$"""
              #nullable enable
              #pragma warning disable CS8618

              namespace EntityNamespace;

              public class Entity
              {
                  public {{propertyType}} PropName { get; set; }
              }

              #pragma warning restore CS8618
              """;

        const string code2 =
            """
            using EntityNamespace;
            using TestBuilderGenerator;

            namespace BuilderNamespace;

            [TestBuilderGenOf<Entity>]
            public partial class EntityBuilder
            {
                public partial Entity Build()
                {
                    throw new System.NotImplementedException();
                }
            }
            """;

        (string fileName, string content) generatedCode = (
            "EntityBuilder.g.cs",
            $$"""
              // <auto-generated/>
              // lang=c#
              #nullable enable
              #pragma warning disable CA5394
              namespace BuilderNamespace;

              public partial class EntityBuilder
              {
                  public static {{propertyType}} DefaultEntityPropName { get; } = {{defaultPropertyType}};
                  private {{propertyType}} _propName = DefaultEntityPropName;
                  public EntityBuilder WithPropName({{propertyType}} propName)
                  {
                      _propName = propName;
                      return this;
                  }

                  public static EntityBuilder Default => new EntityBuilder();
                  public partial EntityNamespace.Entity Build();
              }

              // </auto-generated>
              """);

        await VerifyCS.ValidateSourceGeneratorAsync(
            [code1, code2],
            [_attributeGeneratedCode, generatedCode]);
    }

    [Fact]
    public async Task Not_Generate_AlreadyDefinedDefaultProperty()
    {
        const string code1 =
            """
            #nullable enable
            #pragma warning disable CS8618

            namespace EntityNamespace;

            public class Entity
            {
                public int PredefinedStructProp { get; set; }
            }

            #pragma warning restore CS8618
            """;

        const string code2 =
            """
            using EntityNamespace;
            using TestBuilderGenerator;

            namespace BuilderNamespace;

            [TestBuilderGenOf<Entity>]
            public partial class EntityBuilder
            {
                public static int DefaultEntityPredefinedStructProp => 11;

                public partial Entity Build()
                {
                    throw new System.NotImplementedException();
                }
            }
            """;

        (string fileName, string content) generatedCode = (
            "EntityBuilder.g.cs",
            """
            // <auto-generated/>
            // lang=c#
            #nullable enable
            #pragma warning disable CA5394
            namespace BuilderNamespace;

            public partial class EntityBuilder
            {
                private int _predefinedStructProp = DefaultEntityPredefinedStructProp;
                public EntityBuilder WithPredefinedStructProp(int predefinedStructProp)
                {
                    _predefinedStructProp = predefinedStructProp;
                    return this;
                }

                public static EntityBuilder Default => new EntityBuilder();
                public partial EntityNamespace.Entity Build();
            }

            // </auto-generated>
            """);

        await VerifyCS.ValidateSourceGeneratorAsync(
            [code1, code2],
            [_attributeGeneratedCode, generatedCode]);
    }

    [Fact]
    public async Task Not_Generate_AlreadyDefinedMethod()
    {
        const string code1 =
            """
            #nullable enable
            #pragma warning disable CS8618

            namespace EntityNamespace;

            public class Entity
            {
                public int PredefinedStructProp { get; set; }
            }

            #pragma warning restore CS8618
            """;

        const string code2 =
            """
            using EntityNamespace;
            using TestBuilderGenerator;

            namespace BuilderNamespace;

            [TestBuilderGenOf<Entity>]
            public partial class EntityBuilder
            {
                public EntityBuilder WithPredefinedStructProp(int predefinedStructProp)
                {
                    throw new System.NotImplementedException();
                }

                public partial Entity Build()
                {
                    throw new System.NotImplementedException();
                }
            }
            """;

        (string fileName, string content) generatedCode = (
            "EntityBuilder.g.cs",
            """
            // <auto-generated/>
            // lang=c#
            #nullable enable
            #pragma warning disable CA5394
            namespace BuilderNamespace;

            public partial class EntityBuilder
            {
                public static int DefaultEntityPredefinedStructProp { get; } = global::System.Random.Shared.Next();
                private int _predefinedStructProp = DefaultEntityPredefinedStructProp;

                public static EntityBuilder Default => new EntityBuilder();
                public partial EntityNamespace.Entity Build();
            }

            // </auto-generated>
            """);

        await VerifyCS.ValidateSourceGeneratorAsync(
            [code1, code2],
            [_attributeGeneratedCode, generatedCode]);
    }
}
