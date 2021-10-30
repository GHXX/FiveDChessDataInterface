using DataInterfaceConsole.Types;
using FiveDChessDataInterface.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataInterfaceConsole.Actions
{
    class PGNImporter : BaseAction
    {
        public override string Name => "Load 5D PGN";

        protected override void Run()
        {
            var defaultFileName = "5dpgn.txt";
            var defaultFilePath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, defaultFileName);
            var fileFound = File.Exists(defaultFilePath);
            Console.WriteLine(fileFound ?
                $"A pgn file called '{defaultFileName}' was found. If you want to load a different file, please enter the absolute filepath and hit Enter, otherwise, just press Enter:" :
                $"No pgn file called '{defaultFileName}' was found. Please enter the filename and hit Enter:");

            var path = ConsoleNonBlocking.ReadLineBlocking()?.Trim();

            string[] filedata = null;
            if (path != null)
            {
                if (!string.IsNullOrWhiteSpace(path)) // load given path
                {
                    WriteLineIndented($"Loading file from absolute (preferred) path '{path}'.");
                    if (!File.Exists(path))
                    {
                        WriteLineIndented($"No file was found at the path '{path}'. Aborting!");
                        return;
                    }
                    filedata = File.ReadAllLines(path);
                }
                else if (fileFound) // load default
                {
                    WriteLineIndented("Loading file from default location.");
                    filedata = File.ReadAllLines(defaultFilePath);
                }
                else // error
                {
                    WriteLineIndented("No path given. Aborting.");
                    return;
                }            
            }

            filedata = filedata.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            var headerLines = filedata.TakeWhile(line => line.StartsWith("[")).ToArray();
            var turnLines = filedata.Skip(headerLines.Length).ToArray();

            var headerValues = new Dictionary<string, string>();
            var timelines = new List<(string fen, int timelineL, int turnT, bool isBlacksTurn)>();

            foreach (var line in headerLines)
            {
                var m = Regex.Match(line, @"\[(\S*?) ""(.*)""\]");
                if (m.Success) // this is a header value
                {
                    headerValues.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
                else // otherwise read it as fen-line
                {
                    var m2 = Regex.Match(line, @"\[(.*)\]");
                    if (m2.Success)
                    {
                        var splitted = m2.Value.Split(':');
                        timelines.Add((splitted[0], int.Parse(splitted[1]), int.Parse(splitted[2]), splitted[3].ToLowerInvariant() == "b"));
                    }
                    else
                    {
                        throw new FormatException($"Invalid header line encountered which was interpreted as a fen line with data: {line}");
                    }
                }
            }


            var gb = new BaseGameBuilder()

            Console.WriteLine();
        }
    }
}
