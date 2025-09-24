using DataInterfaceConsole.Actions.Settings;

namespace DataInterfaceConsole.Actions.EphemeralSettings;

internal class ManageEphemeralSettings : BaseManageSettingsMenu {
    public override string Name => "Ephemeral Settings and Triggers";

    public override ISettingsContainer SettingsHandler => Program.instance.eh;    
}
