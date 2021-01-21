using System;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    public interface IBaseModel
    {
        IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges);

        T Shard<T>() where T : IModelShard;

        ModelChangeResult Run(ModelCommand command);

        void RaiseEvent(ModelChangeResult result);
    }
}
