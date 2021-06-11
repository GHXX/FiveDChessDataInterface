using FiveDChessDataInterface.MemoryHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FiveDChessDataInterface.Saving
{
    public class SaveHandler
    {
        private readonly PropertyInfo[] propsToSave;
        private readonly DataInterface di;

        public SaveHandler(DataInterface di)
        {
            this.propsToSave = typeof(DataInterface).GetProperties().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(RequiredForSaveAttribute))).ToArray();
            this.di = di;
        }

        public string SaveToJson()
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in this.propsToSave)
            {
                var value = prop.GetValue(this.di);
                var serializationType = prop.PropertyType;
                if (prop.PropertyType.GetGenericTypeDefinition() == typeof(MemoryLocation<>)) // if the property is a memory location
                {
                    value = ((dynamic)value).GetValue();
                    serializationType = prop.PropertyType.GenericTypeArguments[0];
                }

                var serialized = JsonConvert.SerializeObject(value, serializationType, null);
                dict.Add(prop.GetCustomAttribute<RequiredForSaveAttribute>().Name, serialized);
            }

            dict.Add("_cbms", JsonConvert.SerializeObject(this.di.GetChessBoards().Select(x => x.cbm).ToArray()));

            return JsonConvert.SerializeObject(dict);
        }

        public void LoadFromJson(string data, bool throwOnPartialLoad = true)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            foreach (var prop in this.propsToSave)
            {
                var name = prop.GetCustomAttribute<RequiredForSaveAttribute>().Name;
                if (dict.TryGetValue(name, out string val))
                {
                    if (prop.PropertyType.GetGenericTypeDefinition() == typeof(MemoryLocation<>)) // if the property is a memory location
                    {
                        var newValue = JsonConvert.DeserializeObject(val, prop.PropertyType.GenericTypeArguments[0]);
                        var memLoc = (dynamic)prop.GetValue(this.di);
                        memLoc.SetValue((dynamic)newValue);
                    }
                    else
                    {
                        var deserialized = JsonConvert.DeserializeObject(val, prop.PropertyType);
                        prop.SetValue(this.di, deserialized);
                    }
                }
                else if (throwOnPartialLoad)
                {
                    throw new Exception($"Key {name} was not present in the save-dictionary.");
                }
            }

            var width = this.di.MemLocChessBoardSizeWidth.GetValue();
            var height = this.di.MemLocChessBoardSizeHeight.GetValue();
            var newBoards = JsonConvert.DeserializeObject<ChessBoardMemory[]>(dict["_cbms"]);
            this.di.SetChessBoardArray(newBoards.Select(x => new ChessBoard(x, width, height)).ToArray());
        }
    }
}
