using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Commands;

[DebuggerDisplay("{Value}")]
internal class CommandParameter<T> : ICommandParameter<T>
{
    private T? _value;

    public CommandParameter(string name)
    {
        _value = default!;

        IsInitialized = false;
        Name = name;
    }

    public string Name { get; }

    public T Value
    {
        get => _value ?? throw new InvalidOperationException("Command parameter is not initialized");
        private set
        {
            _value = value;
            IsInitialized = true;
        }
    }

    public bool IsInitialized { get; set; }

    public void Set(T value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"{Name} = '{Value}'";
    }
}
