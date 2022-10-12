using Navitski.Crystalized.Model.Generators.Tests.Infrastructure;

namespace Navitski.Crystalized.Model.Generators.Tests
{
    public class ApplicationModelGeneratorTests : GeneratorTestsBase
    {
        [Test]
        public Task ModelWithAllFeaturesTest()
        {
            return Run(files: "AllFeatures.model.json");
        }
    }
}
