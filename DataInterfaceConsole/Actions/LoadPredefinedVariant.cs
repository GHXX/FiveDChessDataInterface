using FiveDChessDataInterface;
using FiveDChessDataInterface.Variants;
using System;

namespace DataInterfaceConsole.Actions
{
    class LoadPredefinedVariant : BaseAction
    {
        public override string Name => "Load Predefined Variant";

        protected override void Run()
        {
            var variants = GithubVariantGetter.GetAllVariants();

            Console.WriteLine("Select a variant from the following:");
            for (int i = 0; i < variants.Length; i++)
            {
                Console.WriteLine($"\t{(i + 1).ToString().PadLeft((int)Math.Ceiling(Math.Log10(variants.Length)))}. {variants[i].Name} by {variants[i].Author}");
            }

            if (int.TryParse(Util.ConsoleReadLineWhile(() => di.IsValid()), out int input) && input > 0 && input <= variants.Length)
            {
                var chosenVariant = variants[input - 1];
                var gb = chosenVariant.GetGameBuilder();
                di.SetChessBoardArrayFromBuilder(gb);
            }
            else
            {
                Console.WriteLine("Invalid input. Not loading any variant.");
            }
        }
    }
}
