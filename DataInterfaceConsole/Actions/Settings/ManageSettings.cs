using DataInterfaceConsole.Actions.Settings;
using DataInterfaceConsole.Types;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions
{
    class ManageSettings : BaseAction
    {
        public override string Name => "Manage Persistent Settings";

        protected override void Run()
        {
            WriteLineIndented("Select one of the settings that can be changed:");
            var settings = Program.instance.sh.GetSettings();
            var width = (int)Math.Log10(settings.Length) + 1;
            WriteLineIndented(settings.SelectMany((s, i) => $"[{(i + 1).ToString().PadLeft(width)}] {s.Name}: {s.Description}\n\tCurrent value: {s.GetValueAsString()}".Split("\n")));

            if (int.TryParse(ConsoleNonBlocking.ReadLineBlocking(), out int input) && input > 0 && input <= settings.Length)
            {
                var chosenSetting = settings[input - 1];
                if (chosenSetting is SettingsValueWhitelisted<string> sv)
                {
                    WriteLineIndented("The following values are available. Please select the new desired value, and press enter:", 2);

                    var allowedValues = sv.AllowedValues;
                    var width2 = (int)Math.Log10(allowedValues.Length) + 1;
                    WriteLineIndented(allowedValues.Select((s, i) => $"[{(i + 1).ToString().PadLeft(width2)}] {s}"), 2);
                    if (int.TryParse(ConsoleNonBlocking.ReadLineBlocking(), out int input2) && input2 > 0 && input2 <= allowedValues.Length)
                    {
                        sv.SetValue(allowedValues[input2 - 1]);
                    }
                    else
                    {
                        WriteLineIndented("Invalid input. Setting was left unchanged.", 2);
                        return;
                    }
                }
                else if (chosenSetting is SettingsValuePrimitive<int?> sv2)
                {
                    WriteLineIndented($"Please enter an integer to be set as the new value, or enter 'reset' to reset the setting:");
                    var str = ConsoleNonBlocking.ReadLineBlocking();
                    if (int.TryParse(str, out int input2))
                    {
                        sv2.SetPrimitive(input2);
                    }
                    else if (str.Replace("\"", null).Replace("'", null).ToLowerInvariant() == "reset")
                    {
                        sv2.SetPrimitive(null);
                    }
                    else
                    {
                        WriteLineIndented("Invalid input format. Setting was left unchanged.", 2);
                        return;
                    }
                }

                else
                {
                    throw new NotImplementedException("This setting type has not been implemented yet!");
                }
                Program.instance.sh.Save();
            }
            else
            {
                WriteLineIndented("Invalid setting chosen. Aborting.");
            }
        }
    }
}
