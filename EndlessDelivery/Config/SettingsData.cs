using EndlessDelivery.Saving;

namespace EndlessDelivery.Config;

public class SettingsData
{
    public static readonly SaveFile<SettingsData> SettingsFile = SaveFile.RegisterFile(new SaveFile<SettingsData>("local_times.json"));

    public int StartWave;
}
