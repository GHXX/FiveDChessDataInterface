using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings
{
    sealed class SettingsValue<T> : ISettingsValue
    {
        private readonly Dictionary<string, T> valueMap;
        private readonly Func<string, T> lambda;

        public bool AcceptsAnyValue => this.lambda != null;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public T Value { get; private set; }


        private SettingsValue(string id, string name, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public SettingsValue(string id, string name, string description, Dictionary<string, T> valueMap) : this(id, name, description)
        {
            this.valueMap = valueMap;
        }
        public SettingsValue(string id, string name, string description, Func<string, T> lambda) : this(id, name, description)
        {
            this.lambda = lambda;
        }

        public string[] GetAllowedValues() => this.valueMap?.Keys?.ToArray();

        public void SetValueDirect(JToken newValue)
        {
            this.Value = newValue.Value<T>();
        }
        public void SetValueWithConversion(string newValue)
        {
            if (this.valueMap != null)
            {
                if (this.valueMap.TryGetValue(newValue, out T val))
                {
                    this.Value = val;
                }
                else
                {
                    Util.WriteColored($"Attempted to set the setting '{this.Id}' to '{newValue}', which is an invalid value.\n" +
                        $"The value was not changed and will keep its default value.", ConsoleColor.Red);
                }
            }
            else if (this.lambda != null)
            {
                this.Value = this.lambda.Invoke(newValue);
            }
            else
            {
                throw new InvalidOperationException("Neither lambda nor valueMap are set!");
            }
        }

        public object GetValue()
        {
            return this.Value;
        }

        public string GetValueAsString()
        {
            return this.Value.ToString();
        }
    }
}
