using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataInterfaceConsole.Actions.Settings
{
    class SettingsHandler
    {
        const string settingsFilePath = "settings.json";

        public ISettingsValue[] GetSettings() => this.settingsStore.Values.ToArray();

        private readonly Dictionary<string, ISettingsValue> settingsStore = new Dictionary<string, ISettingsValue>(); // string : SettingsValue<T>
        private void AddSetting(ISettingsValue s)
        {
            this.settingsStore.Add(s.Id, s);
        }

        private ISettingsValue GetSetting(string Id) => this.settingsStore[Id];


        public SettingsHandler()
        {
            AddSetting(new SettingsValueWhitelisted<string>("ForceTimetravelAnimationValue", "Force timetravel animation value", "Whether or not to force the timetravel button to a certain state",
                new[] { "ignore", "always_on", "always_off" }, "ignore"));
        }


        public static SettingsHandler LoadOrCreateNew()
        {
            var sh = new SettingsHandler();

            if (File.Exists(settingsFilePath))
            {
                var str = File.ReadAllText(settingsFilePath);
                var jObj = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(str);
                foreach (var (key, value) in jObj)
                {
                    var s = sh.GetSetting(key);
                    s.SetValueDirect(value);
                }
                Console.WriteLine("Settings loaded.");
            }

            return sh;
        }

        public void Save()
        {
            var str = JsonConvert.SerializeObject(this.settingsStore.ToDictionary(x => x.Key, x => x.Value.GetValue()));
            File.WriteAllText(settingsFilePath, str);
            Console.WriteLine("Settings saved.");
        }

        public void Tick()
        {
            var di = Program.instance.di;
            if (!di.IsValid())
                return;

            try
            {
                var ttanim = (this.settingsStore["ForceTimetravelAnimationValue"] as SettingsValueWhitelisted<string>).Value;
                if (ttanim != "ignore")
                    di.MemLocTimeTravelAnimationEnabled.SetValue(ttanim == "always_on" ? 1 : 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while ticking settings handler:\n{ex}");
            }
        }
    }
}
