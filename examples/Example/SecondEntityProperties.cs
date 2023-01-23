namespace Example.Model.Entities
{
    public enum SecondEntityEnum : int
    {
        None = 0,
        First = 1,
        Second = 2,
    }

    partial record SecondEntityProperties
    {
        public SecondEntityEnum EnumProperty
        {
            get => (SecondEntityEnum)IntProperty;
            init => IntProperty = (int)value;
        }
    }
}
