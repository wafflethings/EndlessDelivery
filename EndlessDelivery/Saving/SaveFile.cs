using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EndlessDelivery.Saving;

public class SaveFile<T> : SaveFile where T : new()
{
    public T Data;

    public SaveFile(string fileName) : base(fileName)
    {
        Data = new T();
    }

    protected virtual string Serialize(T value) => JsonConvert.SerializeObject(value);
    protected virtual T Deserialize(string value) => JsonConvert.DeserializeObject<T>(value);

    protected override void LoadData()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }

        Data = Deserialize(File.ReadAllText(FilePath));
    }

    protected override void SaveData()
    {
        File.WriteAllText(FilePath, Serialize(Data));
    }
}

public abstract class SaveFile
{
    private static string AppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wafflethings", Plugin.Name);
    protected string FilePath => Path.Combine(AppData, _fileName);
    private string _fileName;
    private static readonly List<SaveFile> s_saveFiles = new();

    protected SaveFile(string fileName)
    {
        _fileName = fileName;
    }

    protected abstract void LoadData();
    protected abstract void SaveData();

    public static void SaveAll()
    {
        if (!Directory.Exists(AppData))
        {
            Directory.CreateDirectory(AppData);
        }

        foreach (SaveFile file in s_saveFiles)
        {
            file.SaveData();
        }
    }

    public static SaveFile<T> RegisterFile<T>(SaveFile<T> file) where T : new()
    {
        file.LoadData();
        s_saveFiles.Add(file);
        return file;
    }
}
