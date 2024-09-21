using EndlessDelivery.Saving;

namespace EndlessDelivery.Config;

public class SettingsData
{
    public static readonly SaveFile<SettingsData> SettingsFile = SaveFile.RegisterFile(new SaveFile<SettingsData>("settings.json"));

    public int StartWave;
}
