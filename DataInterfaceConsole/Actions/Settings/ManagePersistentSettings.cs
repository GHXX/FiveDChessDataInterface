using DataInterfaceConsole.Actions.Settings;
using DataInterfaceConsole.Types;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions;

internal class ManagePersistentSettings : BaseManageSettingsMenu {
    public override string Name => "Manage Persistent Settings";

    public override ISettingsContainer SettingsHandler => Program.instance.sh;
}
