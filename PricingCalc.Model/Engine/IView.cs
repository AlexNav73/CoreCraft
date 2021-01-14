using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal interface IView
    {
        IModel UnsafeModel { get; }

        MutateResult Mutate(Action<IModel> action);

        MutateResult Apply(IWritableModelChanges changes);
    }
}
