using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings;

internal class PersistentSettingsContainer : ISettingsContainer {
    private const string settingsFilePath = "settings.json";

    public ISettingsValue[] GetSettings() => this.settingsStore.Values.ToArray();

    private readonly Dictionary<string, ISettingsValue> settingsStore = new Dictionary<string, ISettingsValue>(); // string : SettingsValue<T>
    private void AddSetting(ISettingsValue s) {
        this.settingsStore.Add(s.Id, s);
    }

    private ISettingsValue GetSetting(string Id) => this.settingsStore[Id];


    public PersistentSettingsContainer() {
        AddSetting(new SettingsValueWhitelisted<string>("ForceTimetravelAnimationValue", "Force timetravel animation value", "Whether or not to force the timetravel button to a certain state",
            ["ignore", "always_on", "always_off"], "ignore"));

        AddSetting(new SettingsValuePrimitive<int?>("Clock1BaseTime", "Short Timer Base Time", "The base time of the first clock in total seconds", null));
        AddSetting(new SettingsValuePrimitive<int?>("Clock1Increment", "Short Timer Increment", "The increment of the first clock in seconds", null));
        AddSetting(new SettingsValuePrimitive<int?>("Clock2BaseTime", "Medium Timer Base Time", "The base time of the second clock in total seconds", null));
        AddSetting(new SettingsValuePrimitive<int?>("Clock2Increment", "Medium Timer Increment", "The increment of the second clock in seconds", null));
        AddSetting(new SettingsValuePrimitive<int?>("Clock3BaseTime", "Long Timer Base Time", "The base time of the third clock in total seconds", null));
        AddSetting(new SettingsValuePrimitive<int?>("Clock3Increment", "Long Timer Increment", "The increment of the third clock in seconds", null));
    }


    public static PersistentSettingsContainer LoadOrCreateNew() {
        var sh = new PersistentSettingsContainer();

        if (File.Exists(settingsFilePath)) {
            var str = File.ReadAllText(settingsFilePath);
            var jObj = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(str);
            foreach (var (key, value) in jObj) {
                var s = sh.GetSetting(key);
                s.SetValueDirect(value);
            }
            Console.WriteLine("Settings loaded.");
        }

        return sh;
    }

    void ISettingsContainer.OnSettingsChanged() => Save();
    public void Save() {
        var str = JsonConvert.SerializeObject(this.settingsStore.ToDictionary(x => x.Key, x => x.Value.GetValue()), Formatting.Indented);
        File.WriteAllText(settingsFilePath, str);
        Console.WriteLine("Settings saved.");
    }

    public void Tick() {
        var di = Program.instance.di;
        if (di == null || !di.IsValid())
            return;

        try {
            var ttanim = (this.settingsStore["ForceTimetravelAnimationValue"] as SettingsValueWhitelisted<string>).Value;
            if (ttanim != "ignore")
                di.MemLocTimeTravelAnimationEnabled.SetValue(ttanim == "always_on" ? 1 : 0);

            var sv1t = (this.settingsStore["Clock1BaseTime"] as SettingsValuePrimitive<int?>).Value;
            if (sv1t.HasValue)
                di.MemLocClock1BaseTime.SetValue(sv1t.Value);
            else
                di.MemLocClock1BaseTime.RestoreOriginal();

            var sv1i = (this.settingsStore["Clock1Increment"] as SettingsValuePrimitive<int?>).Value;
            if (sv1i.HasValue)
                di.MemLocClock1Increment.SetValue(sv1i.Value);
            else
                di.MemLocClock1Increment.RestoreOriginal();

            var sv2t = (this.settingsStore["Clock2BaseTime"] as SettingsValuePrimitive<int?>).Value;
            if (sv2t.HasValue)
                di.MemLocClock2BaseTime.SetValue(sv2t.Value);
            else
                di.MemLocClock2BaseTime.RestoreOriginal();

            var sv2i = (this.settingsStore["Clock2Increment"] as SettingsValuePrimitive<int?>).Value;
            if (sv2i.HasValue)
                di.MemLocClock2Increment.SetValue(sv2i.Value);
            else
                di.MemLocClock2Increment.RestoreOriginal();

            var sv3t = (this.settingsStore["Clock3BaseTime"] as SettingsValuePrimitive<int?>).Value;
            if (sv3t.HasValue)
                di.MemLocClock3BaseTime.SetValue(sv3t.Value);
            else
                di.MemLocClock3BaseTime.RestoreOriginal();

            var sv3i = (this.settingsStore["Clock3Increment"] as SettingsValuePrimitive<int?>).Value;
            if (sv3i.HasValue)
                di.MemLocClock3Increment.SetValue(sv3i.Value);
            else
                di.MemLocClock3Increment.RestoreOriginal();



        } catch (Exception ex) {
            Console.WriteLine($"Error while ticking settings handler:\n{ex.ToSanitizedString()}");
        }
    }
}
