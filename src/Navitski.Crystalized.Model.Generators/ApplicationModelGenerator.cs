using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Navitski.Crystalized.Model.Generators;

[Generator(LanguageNames.CSharp)]
internal partial class ApplicationModelGenerator : GeneratorBase
{
    protected override void ExecuteInternal(
        SourceProductionContext context,
        string assemblyName,
        ImmutableArray<(string name, string content)> files)
    {
        var options = new JsonSerializerSettings();
        var serializer = JsonSerializer.Create(options);

        foreach (var file in files)
        {
            ModelScheme modelScheme = null;

            using (var stringStream = new StringReader(file.content))
            using (var reader = new JsonTextReader(stringStream))
            {
                modelScheme = serializer.Deserialize<ModelScheme>(reader);
            }

            if (modelScheme == null)
            {
                throw new InvalidOperationException($"Failed to deserialize model file [{file.name}]");
            }

            using (var writer = new StringWriter())
            using (var code = new IndentedTextWriter(writer, "    "))
            {
                code.Preambula();
                Generate(assemblyName, file.name, code, modelScheme);

                AddSourceFile(context, file.name, writer.ToString());
            }
        }
    }

    private void Generate(string assemblyName, string modelName, IndentedTextWriter code, ModelScheme modelScheme)
    {
        var @namespace = $"{assemblyName}.{modelName}";

        code.WriteLine($"namespace {@namespace}");
        code.Block(() =>
        {
            code.WriteLine("using Navitski.Crystalized.Model.Engine;");
            code.WriteLine("using Navitski.Crystalized.Model.Engine.Core;");
            code.WriteLine("using Navitski.Crystalized.Model.Engine.ChangesTracking;");
            code.WriteLine("using Navitski.Crystalized.Model.Engine.Lazy;");
            code.WriteLine("using Navitski.Crystalized.Model.Engine.Persistence;");
            code.WriteLine($"using {@namespace}.Entities;");
            code.EmptyLine();

            GenerateModelShards(code, modelScheme.Shards, @namespace);
            GenerateStorages(code, modelScheme.Shards);
        });
        code.EmptyLine();

        code.WriteLine($"namespace {@namespace}.Entities");
        code.Block(() =>
        {
            GenerateEntities(code, modelScheme.Shards);
        });
    }

    private string DefineProperty(Property prop, string accessors)
    {
        return Property(prop.IsNullable ? $"{prop.Type}?" : prop.Type, prop.Name, accessors);
    }

    private static string Type(Collection collection) => $"Collection<{collection.EntityType}, {PropertiesType(collection.EntityType)}>";

    private static string Type(Relation relation) => $"Relation<{relation.ParentType}, {relation.ChildType}>";

    private static string ChangesType(Collection collection) => $"CollectionChangeSet<{collection.EntityType}, {PropertiesType(collection.EntityType)}>";

    private static string ChangesType(Relation relation) => $"RelationChangeSet<{relation.ParentType}, {relation.ChildType}>";

    private static string PropertiesType(Entity entitiy) => PropertiesType(entitiy.Name);

    private static string PropertiesType(string type) => $"{type}Properties";

    private static string ToCamelCase(string value)
    {
        if (value.Length > 1)
        {
            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }

        return value;
    }
}
