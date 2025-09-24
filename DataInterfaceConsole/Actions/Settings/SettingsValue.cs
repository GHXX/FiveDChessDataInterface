using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings;

internal sealed class SettingsValue<T> : ISettingsValue {
    private readonly Dictionary<string, T> valueMap;
    private readonly Func<string, T> lambda;

    public bool AcceptsAnyValue => this.lambda != null;

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public T Value { get; private set; }

    public bool HideOutputValue { get; init; } = false;

    private SettingsValue(string id, string name, string description) {
        Id = id;
        Name = name;
        Description = description;
    }

    public SettingsValue(string id, string name, string description, Dictionary<string, T> valueMap) : this(id, name, description) {
        this.valueMap = valueMap;
    }
    public SettingsValue(string id, string name, string description, Func<string, T> lambda) : this(id, name, description) {
        this.lambda = lambda;
    }

    public string[] GetAllowedValues() => this.valueMap?.Keys?.ToArray();

    public void SetValueDirect(JToken newValue) {
        Value = newValue.Value<T>();
    }
    public void SetValueWithConversion(string newValue) {
        if (this.valueMap != null) {
            if (this.valueMap.TryGetValue(newValue, out T val)) {
                Value = val;
            } else {
                Util.WriteColored($"Attempted to set the setting '{Id}' to '{newValue}', which is an invalid value.\n" +
                    $"The value was not changed and will keep its default value.", ConsoleColor.Red);
            }
        } else if (this.lambda != null) {
            Value = this.lambda.Invoke(newValue);
        } else {
            throw new InvalidOperationException("Neither lambda nor valueMap are set!");
        }
    }

    public object GetValue() {
        return Value;
    }

    public string GetValueAsString() {
        return Value.ToString();
    }
}
