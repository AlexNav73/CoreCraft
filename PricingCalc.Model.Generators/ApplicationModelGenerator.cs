using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace PricingCalc.Model.Generators
{
    [Generator]
    internal partial class ApplicationModelGenerator : GeneratorBase
    {
        protected override void ExecuteInternal(GeneratorExecutionContext context, IndentedTextWriter code)
        {
            var compilation = (CSharpCompilation)context.Compilation;
            var file = context.AdditionalFiles.SingleOrDefault();
            if (file != null)
            {
                var options = new JsonSerializerSettings();
                var serializer = JsonSerializer.Create(options);
                var modelScheme = serializer.Deserialize<ModelScheme>(new JsonTextReader(new StringReader(File.ReadAllText(file.Path))));

                Generate(compilation, code, modelScheme);
            }
        }

        private void Generate(CSharpCompilation compilation, IndentedTextWriter code, ModelScheme modelScheme)
        {
            Preambula(code);

            var totalShards = modelScheme.AppModel.Shards.Union(modelScheme.UserSettings.Shards);

            code.WriteLine($"namespace {compilation.AssemblyName}.Model");
            Block(code, () =>
            {
                code.WriteLine("using PricingCalc.Model.Engine;");
                code.WriteLine("using PricingCalc.Model.Engine.Core;");
                code.WriteLine("using PricingCalc.Model.Engine.ChangesTracking;");
                code.WriteLine("using PricingCalc.Model.Engine.Persistence;");
                code.WriteLine($"using {compilation.AssemblyName}.Model.Entities;");
                EmptyLine(code);

                GenerateModelShards(code, modelScheme.AppModel, "PricingCalc.Model.AppModel.IApplicationModelShard");
                GenerateModelShards(code, modelScheme.UserSettings, "PricingCalc.Model.UserSettings.IUserSettingsModelShard");
                GenerateStorages(code, totalShards);
            });
            EmptyLine(code);

            code.WriteLine($"namespace {compilation.AssemblyName}.Model.Entities");
            Block(code, () =>
            {
                GenerateEntities(code, totalShards);
            });
        }

        private string GeneratePropertyEqualityComparison(Property prop) => prop switch
        {
            Property p when p.IsNullable && !p.Type.Contains("string") => $"(({prop.Name} == null && other.{prop.Name} == null) || ({prop.Name} != null && {prop.Name}.Equals(other.{prop.Name})))",
            Property p when p.Type.Contains("string") => $"string.Equals({prop.Name}, other.{prop.Name})",
            _ => $"{prop.Name}.Equals(other.{prop.Name})"
        };

        private string DefineProperty(Property prop, string accessors)
        {
            return Property(prop.IsNullable ? $"{prop.Type}?" : prop.Type, prop.Name, accessors);
        }

        private static string CollectionType(Collection collection) => $"Collection<I{collection.Type}, I{EntityPropertiesType(collection.Type)}>";

        private static string RelationType(Relation relation) => $"Relation<I{relation.ParentType}, I{relation.ChildType}>";

        private static string CollectionChangesType(Collection collection) => $"CollectionChanges<I{collection.Type}, I{EntityPropertiesType(collection.Type)}>";

        private static string RelationChangesType(Relation relation) => $"RelationCollectionChanges<I{relation.ParentType}, I{relation.ChildType}>";

        private static string EntityPropertiesType(string type) => $"{type}Properties";
    }
}
