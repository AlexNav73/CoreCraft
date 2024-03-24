namespace CoreCraft.SourceGeneration.Generators;

internal abstract class GeneratorCommon
{
    protected static string DefineProperty(string type, string name, string accessors = "get; private set;")
    {
        return string.Join(" ", type, name, "{", accessors, "}").Trim();
    }

    protected static string ToCamelCase(string value)
    {
        if (value.Length > 1)
        {
            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }

        return value;
    }
}
