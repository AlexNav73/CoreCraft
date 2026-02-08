namespace CoreCraft.Generators.Tests;

public class VerifyChecksTests
{
    [Test]
    public Task CheckConfig() => VerifyChecks.Run();
}
