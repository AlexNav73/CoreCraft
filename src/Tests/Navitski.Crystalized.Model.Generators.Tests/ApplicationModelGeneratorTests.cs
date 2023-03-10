using Navitski.Crystalized.Model.Generators.Tests.Infrastructure;

namespace Navitski.Crystalized.Model.Generators.Tests
{
    public class ApplicationModelGeneratorTests : GeneratorTestsBase
    {
        [Test]
        public Task ModelWithAllFeaturesTest()
        {
            return Run(verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            },
            files: "AllFeatures.model.json");
        }

        [Test]
        public Task TwoModelFilesTest()
        {
            return Run(verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            },
            files: new[]
            {
                "AllFeatures.model.json",
                "AnotherModelFile.model.json"
            });
        }

        [Test]
        public Task ModelWithVisibilityImplementationsTest()
        {
            return Run(verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            },
            files: "VisibilityImplementations.model.json");
        }
    }
}
