using CoreCraft.Core;

namespace ConsoleDemoApp.Model.Entities
{
    public enum SecondEntityEnum : int
    {
        None = 0,
        First = 1,
        Second = 2,
    }

    partial record SecondEntityProperties : IHaveEntityId<SecondEntity>
    {
        public SecondEntity EntityId { get; init; } = new SecondEntity();

        [Newtonsoft.Json.JsonIgnore]
        public SecondEntityEnum EnumProperty
        {
            get => (SecondEntityEnum)IntProperty;
            init => IntProperty = (int)value;
        }
    }

    partial record FirstEntityProperties : IHaveEntityId<FirstEntity>
    {
        public FirstEntity EntityId { get; init; } = new FirstEntity();
    }
}
