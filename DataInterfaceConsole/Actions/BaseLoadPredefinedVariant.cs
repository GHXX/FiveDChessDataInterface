using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Variants;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions
{
    abstract class BaseLoadPredefinedVariant : BaseAction
    {
        protected abstract bool UseOnlineVariants { get; }

        protected override void Run()
        {
            AssemblyTrapAdvanced at = null;
            int safeBoardLimit = -1;
            try
            {
                if (!this.di.IsMatchRunning()) // if the match isnt running, then use the trap mode to inject it
                {
                    // MemoryUtil.FindMemoryWithWildcards(GetGameHandle(), GetEntryPoint(), (uint)this.GameProcess.MainModule.ModuleMemorySize, new byte?[]{0x5b,0x5f,0x5e,0x41,0x5e,0x5d,0xe9})
                    var trapAddress = MemoryUtil.FindMemoryWithWildcards(this.di.GetGameHandle(), this.di.GetEntryPoint(), (uint)this.di.GameProcess.MainModule.ModuleMemorySize,
                        new byte?[] { 0xe8, null, null, null, null, 0x48, 0x89, 0xf1, 0x48, 0x83, 0xc4, 0x20, 0x5b, 0x5f, 0x5e, 0x41, 0x5e, 0x5d, 0xe9 });

                    var trapLocation = trapAddress.Single().Key - 17;
                    at = this.di.asmHelper.PlaceAssemblyTrapAdvanced(trapLocation);
                    Console.WriteLine("Main thread trapped. Please start a game, and then check back here.");
                    at.WaitTillHit();
                }
                else
                {
                    safeBoardLimit = this.di.MemLocChessArrayCapacity.GetValue();
                    Console.WriteLine($"Be advised that injecting a variant with a size of more than {safeBoardLimit} boards during an ongoing match will possibly lead to a crash.\n" +
                        $"You should consider exiting out to the main menu and rerunning the command.");
                    WaitForIngame();
                }

                if (this.UseOnlineVariants && !GithubVariantGetter.IsCached)
                    WriteLineIndented("Getting variants from github...");

                var variants = this.UseOnlineVariants ? GithubVariantGetter.GetAllVariants(false, false) : GithubVariantGetter.GetAllVariants(true, true);

                WriteLineIndented("Select a variant from the following:" + (at == null ? " (Variants that would may lead to a crash in case they are loaded right now are indicated in yellow)" : ""));
                bool anyCrashed = false;
                var originalConsoleColor = Console.ForegroundColor;
                for (int i = 0; i < variants.Length; i++)
                {
                    if (at == null) // if we are using direct injection, build the variant and check how many boards it has.
                    {
                        try
                        {
                            var boardCount = variants[i].GetGameBuilder().Build().Length;
                            if (boardCount > safeBoardLimit)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            anyCrashed = true;
                        }
                    }
                    WriteLineIndented($"{(i + 1).ToString().PadLeft((int)Math.Ceiling(Math.Log10(variants.Length)))}. {variants[i].Name} by {variants[i].Author}");

                    Console.ForegroundColor = originalConsoleColor;
                }


                if (anyCrashed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteLineIndented(($"It appears that one or more variants cannot be loaded. Attempting to do so will lead to a crash. These are marked in red.\n" +
                                           "If you have not made these variants yourself, consider getting the latest release from github (https://5dr.ghxx.dev).").Split('\n'));
                }
                Console.ForegroundColor = originalConsoleColor;

                if (int.TryParse(Util.ConsoleReadLineWhile(() => this.di.IsValid()), out int input) && input > 0 && input <= variants.Length)
                {
                    var chosenVariant = variants[input - 1];
                    WriteLineIndented($"Loading variant '{chosenVariant.Name}'...");
                    var gb = chosenVariant.GetGameBuilder();
                    this.di.SetChessBoardArrayFromBuilder(gb);
                    WriteLineIndented($"Variant loaded and written to memory.");
                }
                else
                {
                    WriteLineIndented("Invalid input. Not loading any variant.");
                }
            }
            finally
            {
                if (at != null)
                {
                    WriteLineIndented("Untrapped game thread.");
                    at.ReleaseTrap();
                }
            }
        }
    }
}
