using Newtonsoft.Json.Linq;

namespace DataInterfaceConsole.Actions.Settings;

internal class SettingsValuePrimitive<T> : ISettingsValue {
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public T Value { get; private set; }

    public SettingsValuePrimitive(string id, string name, string description, T defaultValue) {
        Id = id;
        Name = name;
        Description = description;
        Value = defaultValue;
    }

    public object GetValue() {
        return Value;
    }

    public void SetPrimitive(T newValue) => Value = newValue;

    public string GetValueAsString() {
        return $"\"{Value}\"";
    }

    public void SetValueDirect(JToken newValue) {
        Value = newValue.Value<T>();
    }
}
