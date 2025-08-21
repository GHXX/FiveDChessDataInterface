namespace DataInterfaceConsole.Actions;

internal class LoadPredefinedOnlineVariant : BaseLoadPredefinedVariant {
    public override string Name => "Load Predefined Variant [ONLINE]";

    protected override bool UseOnlineVariants => true;
}
