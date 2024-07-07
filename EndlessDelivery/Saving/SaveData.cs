using System;
using System.Collections.Generic;
using System.IO;
using EndlessDelivery.Server.ContentFile;
using Newtonsoft.Json;

namespace EndlessDelivery.Saving;

public abstract class SaveData<T> : SaveData
{
    public T Value;

    public override void Save()
    {
        File.WriteAllText(FilePath, Serialize(Value));
    }

    public override void Load()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }

        Value = Deserialize(File.ReadAllText(FilePath));
    }

    public abstract string Serialize(T value);

    public abstract T Deserialize(string value);

    protected SaveData(string fileName) : base(fileName)
    {
    }
}

public abstract class SaveData
{
    private static List<SaveData> s_allSaveData = new();

    public static string AppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wafflethings", "EndlessDelivery");
    public string FilePath => Path.Combine(AppData, FileName);

    public readonly string FileName;

    protected SaveData(string fileName)
    {
        FileName = fileName;
    }

    public static SaveData RegisterData(SaveData data)
    {
        s_allSaveData.Add(data);
        data.Load();
        return data;
    }

    public static void SaveAll()
    {
        foreach (SaveData saveData in s_allSaveData)
        {
            saveData.Save();
        }
    }

    public static void LoadAll()
    {
        if (!Directory.Exists(AppData))
        {
            Directory.CreateDirectory(AppData);
        }

        foreach (SaveData saveData in s_allSaveData)
        {
            saveData.Load();
        }
    }

    public virtual void Save() { }

    public virtual void Load() {}
}
