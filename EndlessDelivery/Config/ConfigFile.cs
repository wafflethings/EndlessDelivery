using AtlasLib.Saving;
using Newtonsoft.Json;

namespace EndlessDelivery.Config;

public class ConfigFile
{
    [JsonIgnore] public static readonly EncryptedSaveFile<ConfigFile> Instance = SaveFile.RegisterFile(new EncryptedSaveFile<ConfigFile>("config.ddenc")) as EncryptedSaveFile<ConfigFile> ?? throw new();
}
