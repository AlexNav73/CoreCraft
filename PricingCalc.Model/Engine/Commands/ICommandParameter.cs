namespace PricingCalc.Model.Engine.Commands
{
    public interface ICommandParameter
    {
        string Name { get; }

        bool IsInitialized { get; set; }
    }

    public interface ICommandParameter<T> : ICommandParameter
    {
        T Value { get; set; }

        void SetValue(T value);
    }
}
