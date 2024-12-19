using System.Collections.Generic;
using AtlasLib.Saving;
using EndlessDelivery.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace EndlessDelivery.Config;

public class ConfigFile
{
    [JsonIgnore] public static readonly SaveFile<ConfigFile> Instance = SaveFile.RegisterFile(new SaveFile<ConfigFile>("config.json", Plugin.Name, new())) ?? throw new();
    [JsonIgnore] public List<SerializableColour> PresentColours {
        get
        {
            _presentColours ??= new List<SerializableColour>(PresentColourUi.DefaultColours);
            return _presentColours;
        }
    }
    [JsonProperty] private List<SerializableColour>? _presentColours;

    public Color GetColour(int index) => PresentColours[index].ToUnity();
}
