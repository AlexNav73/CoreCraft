namespace ConsoleDemoApp.Model.Entities
{
    public enum SecondEntityEnum : int
    {
        None = 0,
        First = 1,
        Second = 2,
    }

    partial record SecondEntityProperties
    {
        [Newtonsoft.Json.JsonIgnore]
        public SecondEntityEnum EnumProperty
        {
            get => (SecondEntityEnum)IntProperty;
            init => IntProperty = (int)value;
        }
    }
}
