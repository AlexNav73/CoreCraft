using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CoreCraft.Generators;

public abstract class GeneratorBase : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
        id: "AMG001",
#pragma warning restore RS2008 // Enable analyzer release tracking
        title: "Exception occurred during generator execution",
        messageFormat: "Message: {0} StackTrace: {1}",
        category: "AMG",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        const string ModelFileName = ".model.json";

        var additionalFilesProvider = context.AdditionalTextsProvider
            .Where(static a => a.Path.EndsWith(ModelFileName))
            .Select(static (a, c) => (name: Path.GetFileName(a.Path).Replace(ModelFileName, ""), content: a.GetText(c)!.ToString()))
            .Collect();

        var assemblyNameProvider = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);

        var additionalFilesAndAssemblyName = additionalFilesProvider.Combine(assemblyNameProvider);

        context.RegisterSourceOutput(additionalFilesAndAssemblyName, (productionContext, sourceContext) => Execute(productionContext, sourceContext.Right, sourceContext.Left));
    }

    public void Execute(
        SourceProductionContext context,
        string assemblyName,
        ImmutableArray<(string name, string content)> files)
    {
        try
        {
            ExecuteInternal(context, assemblyName, files);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(_descriptor, Location.None, ex.Message, ex.StackTrace));
        }
    }

    protected abstract void ExecuteInternal(
        SourceProductionContext context,
        string assemblyName,
        ImmutableArray<(string name, string content)> files);

    protected void AddSourceFile(SourceProductionContext context, string fileName, string content)
    {
        context.AddSource($"{fileName}.g.cs", SourceText.From(content, Encoding.UTF8));
    }

    protected string Property(string type, string name, string accessors = "get; private set;")
    {
        return string.Join(" ", type, name, "{", accessors, "}").Trim();
    }
}
