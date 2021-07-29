namespace DataInterfaceConsole.Actions
{
    class LoadPredefinedOnlineVariant : BaseLoadPredefinedVariant
    {
        public override string Name => "Load Predefined Variant [ONLINE]";

        protected override bool UseOnlineVariants => true;
    }
}
