namespace IGOEnchi.SmartGameLib.models
{
    public struct SGFProperty
    {
        private readonly string name;
        private readonly string value;

        public SGFProperty(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}