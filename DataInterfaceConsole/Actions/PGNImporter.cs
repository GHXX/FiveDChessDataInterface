using DataInterfaceConsole.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataInterfaceConsole.Actions
{
    class PGNImporter : BaseAction
    {
        public override string Name => "Load 5D PGN";

        protected override void Run()
        {
            var defaultFileName = "5dpgn.txt";
            var fileFound = File.Exists(defaultFileName);
            Console.WriteLine(fileFound ? 
                $"A pgn file called '{defaultFileName}' was found. If you want to load a different file, please enter the filename and hit Enter, otherwise, just press Enter:" :
                $"No pgn file called '{defaultFileName}' was found. Please enter the filename and hit Enter:");
            var path = ConsoleNonBlocking.ReadLineBlocking()?.Trim();
            if


            if{
                
            }
        }
    }
}
