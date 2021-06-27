using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings
{
    class SettingsValueWhitelisted<T> : ISettingsValue
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public T[] AllowedValues { get; private set; }

        public T Value { get; private set; }

        public object GetValue()
        {
            return this.Value;
        }

        public void SetValueDirect(JToken newValue)
        {
            var v = newValue.Value<T>() ?? throw new InvalidCastException("Value<T> returned null!");
            SetValue(v);
        }

        public void SetValue(T newValue)
        {
            if (!this.AllowedValues.Contains(newValue))
                throw new InvalidOperationException("The given value is not contained in the whitelist");

            this.Value = newValue;
        }

        public string GetValueAsString()
        {
            return this.Value.ToString();
        }

        public SettingsValueWhitelisted(string id, string name, string description, T[] allowedValues, T defaultValue)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.AllowedValues = allowedValues;
            SetValue(defaultValue);
        }
    }
}
