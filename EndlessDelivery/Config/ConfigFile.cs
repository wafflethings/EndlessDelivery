using System;
using AtlasLib.Saving;
using Newtonsoft.Json;

namespace EndlessDelivery.Config;

public class ConfigFile
{
    [JsonIgnore]
    public static EncryptedSaveFile<ConfigFile> Instance = SaveFile.RegisterFile(new EncryptedSaveFile<ConfigFile>()) as EncryptedSaveFile<ConfigFile> ?? throw new Exception("There is absolutely no reason this should happen.");

    public int StartWave { get; set; } = 0;
}
