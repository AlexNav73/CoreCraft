using Navitski.Crystalized.Model.Generators.Tests.Infrastructure;

namespace Navitski.Crystalized.Model.Generators.Tests
{
    public class ApplicationModelGeneratorTests : GeneratorTestsBase
    {
        [Test]
        public Task ModelWithAllFeaturesTest()
        {
            return Run(files: "AllFeatures.model.json", verification: result =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Diagnostics.Count(), Is.EqualTo(0));
            });
        }
    }
}
