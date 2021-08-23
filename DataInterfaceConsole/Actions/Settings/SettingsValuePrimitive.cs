using Newtonsoft.Json.Linq;

namespace DataInterfaceConsole.Actions.Settings
{
    class SettingsValuePrimitive<T> : ISettingsValue
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public T Value { get; private set; }

        public SettingsValuePrimitive(string id, string name, string description, T defaultValue)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Value = defaultValue;
        }

        public object GetValue()
        {
            return this.Value;
        }

        public void SetPrimitive(T newValue) => this.Value = newValue;

        public string GetValueAsString()
        {
            return $"\"{this.Value}\"";
        }

        public void SetValueDirect(JToken newValue)
        {
            this.Value = newValue.Value<T>();
        }
    }
}
