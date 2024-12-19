using System;
using System.IO;
using System.Text;
using AtlasLib.Saving;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace EndlessDelivery.Config;

public class EncryptedSaveFile<T> : SaveFile<T> where T : new()
{
    private const string Key = "Please don't cheat this is just a mod :( I will ban you";

    public EncryptedSaveFile(string name, T defaultValue) : base(name, Plugin.Name, defaultValue)
    {
    }

    protected override string Serialize(T value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        bytes = Encoding.UTF8.GetBytes(BitConverter.ToString(bytes));
        ProcessBytes(ref bytes);
        return Encoding.UTF8.GetString(bytes);
    }

    protected override T Deserialize(string value)
    {
        byte[] stringBytes = Encoding.UTF8.GetBytes(value);
        ProcessBytes(ref stringBytes);
        string[] split = Encoding.UTF8.GetString(stringBytes).Split('-');
        byte[] bytes = new byte[split.Length];

        for (int i = 0; i < split.Length; i++)
        {
            bytes[i] = Convert.ToByte(split[i], 16);
        }

        T deserialized = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        Plugin.Log.LogMessage($"File: {JsonConvert.SerializeObject(deserialized)}");
        return deserialized ?? new T();
    }

    private void ProcessBytes(ref byte[] bytes)
    {
        return;

        int keyIndex = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(bytes[i] ^ Key[keyIndex++]); ;
            if (keyIndex >= Key.Length - 1)
            {
                keyIndex = 0;
            }
        }
    }
}
