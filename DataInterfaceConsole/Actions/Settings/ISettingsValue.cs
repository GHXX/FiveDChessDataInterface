using Newtonsoft.Json.Linq;

namespace DataInterfaceConsole.Actions.Settings;

public interface ISettingsValue {
    string Id { get; }
    string Name { get; }
    string Description { get; }
    void SetValueDirect(JToken newValue);
    object GetValue();
    string GetValueAsString();
    bool HideOutputValue { get; }
}
