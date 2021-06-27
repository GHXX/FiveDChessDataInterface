using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataInterfaceConsole.Actions.Settings
{
    public interface ISettingsValue
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public void SetValueDirect(JToken newValue);
        public object GetValue();
        public string GetValueAsString();
    }
}
