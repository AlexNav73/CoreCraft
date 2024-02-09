using CoreCraft.Generators.Tests.Infrastructure;

namespace CoreCraft.Generators.Tests
{
    public class ApplicationModelGeneratorTests : GeneratorTestsBase
    {
        [Test]
        [TestCase("AllFeatures.model.json")]
        [TestCase("VisibilityImplementations.model.json")]
        [TestCase("DeferLoading.model.json")]
        public Task OneModelFileSnapshotTest(string file)
        {
            return Run(verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            },
            files: file);
        }

        [Test]
        public Task TwoModelFilesSnapshotTest()
        {
            return Run(verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            },
            files:
            [
                "AllFeatures.model.json",
                "AnotherModelFile.model.json"
            ]);
        }
    }
}
