using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Tests.Infrastructure;
using PricingCalc.Model.Tests.Infrastructure.Model;

namespace PricingCalc.Model.Tests.MemoryTests.Commands
{
    public class AddLotOfEntitiesCommand : ModelCommand<FakeModel>
    {
        public AddLotOfEntitiesCommand(FakeModel model)
            : base(model)
        {
        }

        protected override void ExecuteInternal(IModel model)
        {
            var modelShard = model.Shard<IFakeModelShard>();

            for (var i = 0; i < 100; i++)
            {
                var first = modelShard.FirstCollection.Create()
                    .WithInit(p => p.NonNullableStringProperty = "test")
                    .Build();
                var second = modelShard.SecondCollection.Create()
                    .WithInit(p => p.FloatProperty = 0.5f)
                    .Build();

                modelShard.ManyToManyRelation.Add(first, second);
                modelShard.ManyToOneRelation.Add(first, second);
                modelShard.OneToManyRelation.Add(first, second);
                modelShard.OneToOneRelation.Add(first, second);
            }
        }
    }
}
