using DataInterfaceConsole.Actions.EphemeralSettings;
using DataInterfaceConsole.Actions.Settings;
using DataInterfaceConsole.Types;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions;
internal abstract class BaseManageSettingsMenu : BaseAction {
    public abstract ISettingsContainer SettingsHandler { get; }

    protected override void Run() {
        WriteLineIndented("Select one of the following settings that you would like to change:");
        var settings = SettingsHandler.GetSettings();
        var width = (int)Math.Log10(settings.Length) + 1;
        WriteLineIndented(settings.SelectMany((s, i) => {
            var rv = $"[{(i + 1).ToString().PadLeft(width)}] {s.Name}: {s.Description}";
            if (!s.HideOutputValue && s is not SettingsValuePrimitive<Trigger>) {
                rv += $"\n\tCurrent value: {s.GetValueAsString()}";
            }
            return rv.Split("\n");
        }));

        if (int.TryParse(ConsoleNonBlocking.ReadLineBlocking(), out int input) && input > 0 && input <= settings.Length) {
            var chosenSetting = settings[input - 1];
            if (chosenSetting is SettingsValueWhitelisted<string> sv) {
                WriteLineIndented("The following values are available. Please select the new desired value, and press enter:", 2);

                var allowedValues = sv.AllowedValues;
                var width2 = (int)Math.Log10(allowedValues.Length) + 1;
                WriteLineIndented(allowedValues.Select((s, i) => $"[{(i + 1).ToString().PadLeft(width2)}] {s}"), 2);
                if (int.TryParse(ConsoleNonBlocking.ReadLineBlocking(), out int input2) && input2 > 0 && input2 <= allowedValues.Length) {
                    sv.SetValue(allowedValues[input2 - 1]);
                    sv.OnSettingChanged();
                } else {
                    WriteLineIndented("Invalid input. Setting was left unchanged.", 2);
                    return;
                }
            } else if (chosenSetting is SettingsValuePrimitive<int?> sv2) {
                WriteLineIndented($"Please enter an integer to be set as the new value, or enter 'reset' to reset the setting:");
                var str = ConsoleNonBlocking.ReadLineBlocking();
                if (int.TryParse(str, out int input2)) {
                    sv2.SetPrimitive(input2);
                    sv2.OnSettingChanged();
                } else if (str.Replace("\"", null).Replace("'", null).Equals("reset", StringComparison.InvariantCultureIgnoreCase)) {
                    sv2.SetPrimitive(null);
                    sv2.OnSettingChanged();
                } else {
                    WriteLineIndented("Invalid input format. Setting was left unchanged.", 2);
                    return;
                }
            } else if (chosenSetting is SettingsValuePrimitive<string> sv3) {
                WriteLineIndented($"Please enter a string to be set as the new value, or enter 'reset' to reset the setting:");
                var str = ConsoleNonBlocking.ReadLineBlocking();
                sv3.SetPrimitive(str);
                sv3.OnSettingChanged();
            } else if (chosenSetting is SettingsValuePrimitive<Trigger> sv4) {
                sv4.Value.Run();
            } else {
                throw new NotImplementedException("This setting type has not been implemented yet!");
            }
            SettingsHandler.OnSettingsChanged();
        } else {
            WriteLineIndented("Invalid setting chosen. Aborting.");
        }
    }
}
