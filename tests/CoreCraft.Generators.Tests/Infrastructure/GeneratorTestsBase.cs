using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CoreCraft.Generators.Tests.Infrastructure
{
    public abstract class GeneratorTestsBase
    {
        private const string EmptyProgram = @"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";

        protected async Task Run(
            Action<GeneratorDriverRunResult?>? verification = null,
            params string[] files)
        {
            var compilation = CreateCompilation(EmptyProgram);

            var generator = new ApplicationModelGenerator().AsSourceGenerator();
            var additionalFiles = files
                .Select(x => (file: x, content: File.ReadAllText(@$"./TestFiles/{x}")))
                .Select(x => new InMemoryAdditionalText(x.file, x.content))
                .ToImmutableArray();
            GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], additionalTexts: additionalFiles);

            driver = driver.RunGenerators(compilation);

            var runResult = driver.GetRunResult();

            verification?.Invoke(runResult);

            await Verify(runResult)
                .UseDirectory("../VerifiedFiles");
        }

        private static Compilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create(
                "compilation",
                [CSharpSyntaxTree.ParseText(source)],
                [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
