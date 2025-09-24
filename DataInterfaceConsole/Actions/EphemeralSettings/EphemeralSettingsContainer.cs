using DataInterfaceConsole.Actions.Settings;
using FiveDChessDataInterface;
using FiveDChessDataInterface.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataInterfaceConsole.Actions.EphemeralSettings;
internal class EphemeralSettingsContainer : ISettingsContainer {
    public ISettingsValue[] GetSettings() => this.OptionsStore.Values.ToArray();

    private readonly Dictionary<string, ISettingsValue> OptionsStore = new Dictionary<string, ISettingsValue>(); // string : OptionsValue<T>
    private void AddOption(ISettingsValue s) {
        this.OptionsStore.Add(s.Id, s);
    }

    private DataInterface Di => Program.instance.di;

    public EphemeralSettingsContainer() {
        AddOption(new SettingsValueWhitelisted<string>("UndoSubmittedMoves", "Allow Undoing Submitted Moves", "Allows Undoing submitted moves in Singleplayer Games (Local/CPU)",
            ["on", "off"], "off", @this => Di.MemLocUndoMoveReducedByValue.SetValue((byte)(@this.GetValueAsString() == "off" ? 0xFF : 0x00))));
        AddOption(new SettingsValuePrimitive<string>("PasteLobbyCode", "Set private match code", "Sets the ingame private match code used for joining someone else's game", "",
            @this => {
                string[] pieceMap = [
                    ":pawn_white:",
                    ":knight_white:",
                    ":bishop_white:",
                    ":rook_white:",
                    ":queen_white:",
                    ":king_white:",
                    ":pawn_black:",
                    ":knight_black:",
                    ":bishop_black:",
                    ":rook_black:",
                    ":queen_black:",
                    ":king_black:"
                ];

                // this last integer (6) is the amount of already entered digits
                int[] value = @this.GetValueAsString().Trim().Replace("\\", "").Replace("\"", "").Split(" ").Select(x => Array.IndexOf(pieceMap, x)).Append(6).ToArray();
                Di.MemLocJoiningRoomCodeArray.SetValue(new InlineMemoryArray<int>(value));
                @this.SetPrimitive("");
            }) { HideOutputValue = true });
        AddOption(new SettingsValuePrimitive<Trigger>("ResumeGame", "Resume finished Game", "Restores the game to a state where an already finished game can be continued as if it never ended",
            new Trigger(() => {
                foreach (var curItem in new[] { Di.MemLocShowFinishGameButton, Di.MemLocShowEndOfGameDesc, Di.MemLocBackgroundColorChange, Di.MemLocPropertyAtEndOfGame }) {
                    curItem.SetValue(0);
                }
            })));
    }
}
