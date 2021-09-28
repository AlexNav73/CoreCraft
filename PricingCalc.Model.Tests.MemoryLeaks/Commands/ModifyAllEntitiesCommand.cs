using System.Linq;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Tests.Infrastructure;
using PricingCalc.Model.Tests.Infrastructure.Model;

namespace PricingCalc.Model.Tests.MemoryTests.Commands
{
    internal class ModifyAllEntitiesCommand : ModelCommand<FakeModel>
    {
        public ModifyAllEntitiesCommand(FakeModel model)
            : base(model)
        {
        }

        protected override void ExecuteInternal(IModel model)
        {
            var modelShard = model.Shard<IFakeModelShard>();

            foreach (var entity in modelShard.FirstCollection.ToArray())
            {
                modelShard.FirstCollection.Modify(entity, p => p.NonNullableStringProperty = "test2");
            }

            foreach (var entity in modelShard.SecondCollection.ToArray())
            {
                modelShard.SecondCollection.Modify(entity, p => p.FloatProperty = 1f);
            }
        }
    }
}
