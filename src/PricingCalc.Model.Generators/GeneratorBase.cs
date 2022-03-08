using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PricingCalc.Model.Generators;

public abstract class GeneratorBase : ISourceGenerator
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

    public virtual void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            ExecuteInternal(context);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(_descriptor, Location.None, ex.Message, ex.StackTrace));
        }
    }

    protected abstract void ExecuteInternal(GeneratorExecutionContext context);

    protected void AddSourceFile(GeneratorExecutionContext context, string fileName, string content)
    {
        context.AddSource($"{fileName}.g.cs", SourceText.From(content, Encoding.UTF8));
    }

    protected string Property(string type, string name, string accessors = "get; private set;")
    {
        return string.Join(" ", type, name, "{", accessors, "}").Trim();
    }

    protected static string ToCamelCase(string str)
    {
        if (str != string.Empty && char.IsUpper(str[0]))
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        return str;
    }
}
