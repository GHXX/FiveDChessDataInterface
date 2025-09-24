namespace DataInterfaceConsole.Actions.Settings;
internal interface ISettingsContainer {
    ISettingsValue[] GetSettings();
    void OnSettingsChanged() { }
}
