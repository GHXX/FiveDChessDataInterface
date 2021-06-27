using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Variants;
using System;
using System.Threading;

namespace DataInterfaceConsole.Actions
{
    class LoadPredefinedOfflineVariant : BaseLoadPredefinedVariant
    {
        public override string Name => "Load Predefined Variant [OFFLINE]";

        protected override bool UseOnlineVariants => false;
    }
}
