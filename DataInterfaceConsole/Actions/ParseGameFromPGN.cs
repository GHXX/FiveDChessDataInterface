using DataInterfaceConsole.Types;
using FiveDChessDataInterface.Builders;
using System;
using System.Collections.Generic;

namespace DataInterfaceConsole.Actions;
internal class ParseGameFromPGN : BaseAction {
    public override string Name => "Parse Game From PGN";

    protected override void Run() {
        var at = this.di.TrapMainThreadForGameLoad();
        WriteLineIndented("Trap placed - please start a fresh game in the client");
        at.WaitTillHit();

        WriteLineIndented("Enter a PGN, including the starting configuration and moves. Input is presumed complete once a fully empty line is inputted. If you are unsure about the format, please ask on Discord.");

        var pgnLines = new List<string>();
        while (true) {
            var line = ConsoleNonBlocking.ReadLineBlocking().Trim();
            if (string.IsNullOrWhiteSpace(line))
                break;
            pgnLines.Add(line);
        }
        Console.WriteLine("Loading...");
        var pgn = string.Join("\n", pgnLines);
        var gb = BaseGameBuilder.CreateFromFullPgn(pgn);
        this.di.SetChessBoardArrayFromBuilder(gb);


        WriteLineIndented($"Chessboards are located at 0x{this.di.MemLocChessArrayPointer.GetValue().ToString("X16")}");
        at.ReleaseTrap();
        WriteLineIndented("Load complete");
    }
}
