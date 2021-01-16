using System;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine
{
    internal interface IView
    {
        IModel UnsafeModel { get; }

        ModelChangeResult Mutate(Action<IModel> action);

        ModelChangeResult Apply(IWritableModelChanges changes);
    }
}
