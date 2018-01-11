namespace MasterModuleCommon
{
    public struct KeyValuePair<TK, TV>
    {
        public KeyValuePair(TK key, TV value)
        {
            Key = key;
            Value = value;
        }

        public TK Key
        { get; set; }

        public TV Value
        { get; set; }
    }
}
