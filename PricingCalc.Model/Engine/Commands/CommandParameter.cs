using System.Diagnostics;

namespace PricingCalc.Model.Engine.Commands
{
    [DebuggerDisplay("{Value}")]
    internal class CommandParameter<T> : ICommandParameter<T>
    {
        public CommandParameter(string name)
        {
            Value = default!;
            IsInitialized = false;
            Name = name;
        }

        public string Name { get; }

        public T Value { get; set; }

        public bool IsInitialized { get; set; }

        public void SetValue(T value)
        {
            Value = value;
            IsInitialized = true;
        }

        public override string ToString()
        {
            return $"{Name} = '{Value}'";
        }
    }
}
