using System.Collections.Generic;
using System.IO;
using System.Linq;
using AtlasLib.Utils;
using EndlessDelivery.Config;
using Newtonsoft.Json;

namespace EndlessDelivery.Config;

public class Option
{
    public static string SavePath => Path.Combine(PathUtils.ModPath(), "Savedata");
    public static string OptionFilePath => Path.Combine(SavePath, "settings.json");
    protected static Dictionary<string, object> AllOptionValues = new();

    public static Dictionary<string, Option> AllOptions = new()
    {
        { "start_wave", new Option<long>("start_wave", 0) },
        { "disable_copyrighted_music", new Option<bool>("disable_copyrighted_music", false) }
    };

    public static T GetValue<T>(string id) => (AllOptions[id] as Option<T>).Value;

    public static void Save()
    {
            Directory.CreateDirectory(SavePath);
            File.WriteAllText(OptionFilePath, JsonConvert.SerializeObject(AllOptionValues));
        }
        
    public static void Load()
    {
            if (File.Exists(OptionFilePath))
            {
                AllOptionValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(OptionFilePath));
            }
        }
}
    
public class Option<T> : Option
{
    private string _id;

    public T Value
    {
        get
        {
                if (!AllOptionValues.ContainsKey(_id))
                {
                    AllOptionValues.Add(_id, _defaultValue);
                }

                return (T)AllOptionValues[_id];
            }

        set
        {
                if (!AllOptionValues.ContainsKey(_id))
                {
                    AllOptionValues.Add(_id, value);
                    return;
                }

                AllOptionValues[_id] = value;
            }
    }

    private T _defaultValue;
        
    public Option(string id, T defaultValue)
    {
            _id = id;
            _defaultValue = defaultValue;
        }
}