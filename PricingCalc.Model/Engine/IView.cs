using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal interface IView
    {
        IModel UnsafeModel { get; }

        event EventHandler<ModelChangedEventArgs> Changed;

        void Mutate(Action<IModel> action);

        void Apply(IWritableModelChanges changes);
    }
}
