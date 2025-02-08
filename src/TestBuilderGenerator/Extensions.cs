namespace TestBuilderGenerator;

public static class Extensions
{
    public static string ToCamelCase(this string str) =>
        str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
}
