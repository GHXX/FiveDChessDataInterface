using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings;

internal class SettingsValueWhitelisted<T> : ISettingsValue {
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public T[] AllowedValues { get; private set; }

    public T Value { get; private set; }

    public object GetValue() {
        return Value;
    }

    public void SetValueDirect(JToken newValue) {
        var v = newValue.Value<T>() ?? throw new InvalidCastException("Value<T> returned null!");
        SetValue(v);
    }

    public void SetValue(T newValue) {
        if (!AllowedValues.Contains(newValue))
            throw new InvalidOperationException("The given value is not contained in the whitelist");

        Value = newValue;
    }

    public string GetValueAsString() {
        return Value.ToString();
    }

    public SettingsValueWhitelisted(string id, string name, string description, T[] allowedValues, T defaultValue) {
        Id = id;
        Name = name;
        Description = description;
        AllowedValues = allowedValues;
        SetValue(defaultValue);
    }
}
