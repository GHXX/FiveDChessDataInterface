using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Variants;
using System;
using System.Threading;

namespace DataInterfaceConsole.Actions
{
    class LoadPredefinedOnlineVariant : BaseLoadPredefinedVariant
    {
        public override string Name => "Load Predefined Variant [ONLINE]";

        protected override bool UseOnlineVariants => true;
    }
}
