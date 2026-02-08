using System.Runtime.CompilerServices;

namespace CoreCraft.Generators.Tests;

public static class Initializer
{

    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
