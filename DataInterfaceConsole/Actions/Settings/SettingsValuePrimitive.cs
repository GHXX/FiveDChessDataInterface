using Newtonsoft.Json.Linq;
using System;

namespace DataInterfaceConsole.Actions.Settings;

internal class SettingsValuePrimitive<T> : ISettingsValue {
    private readonly Action<SettingsValuePrimitive<T>> onSettingChanged;

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public T Value { get; private set; }
    public bool HideOutputValue { get; init; } = false;

    public SettingsValuePrimitive(string id, string name, string description, T defaultValue, Action<SettingsValuePrimitive<T>> onSettingChanged = null) {
        Id = id;
        Name = name;
        Description = description;
        Value = defaultValue;
        this.onSettingChanged = onSettingChanged;
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

    public void OnSettingChanged() => this.onSettingChanged?.Invoke(this);
}
