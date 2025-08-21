namespace DataInterfaceConsole.Actions;

internal class LoadPredefinedOfflineVariant : BaseLoadPredefinedVariant {
    public override string Name => "Load Predefined Variant [OFFLINE]";

    protected override bool UseOnlineVariants => false;
}
