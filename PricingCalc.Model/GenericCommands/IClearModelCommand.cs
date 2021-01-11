using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.Commands;

namespace PricingCalc.Model.GenericCommands
{
    public interface IClearModelCommand<TModel> : IModelCommand
        where TModel : IBaseModel
    {
    }
}
