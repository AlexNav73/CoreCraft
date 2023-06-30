using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            [CallerMemberName] string methodName = "",
            Action<GeneratorDriverRunResult?>? verification = null,
            params string[] files)
        {
            var compilation = CreateCompilation(EmptyProgram);

            var generator = new ApplicationModelGenerator().AsSourceGenerator();
            var additionalFiles = files
                .Select(x => (file: x, content: File.ReadAllText(@$"./TestFiles/{x}")))
                .Select(x => new InMemoryAdditionalText(x.file, x.content))
                .ToImmutableArray();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, additionalTexts: additionalFiles);

            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var _);

            var runResult = driver.GetRunResult();

            verification?.Invoke(runResult);

            foreach (var tree in runResult.GeneratedTrees)
            {
                var generatedFileName = Path.GetFileName(tree.FilePath).Replace('.', '_');
                var generatedSource = tree.GetText().ToString();
                var fileName = $"{GetType().Name}_{methodName}_{generatedFileName}";

                await Verify(generatedSource)
                    .UseDirectory("../VerifiedFiles")
                    .UseFileName(fileName);
            }
        }

        private static Compilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create(
                "compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
