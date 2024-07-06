using EndlessDelivery.Common.ContentFile;
using Newtonsoft.Json;

namespace EndlessDelivery.Saving;

public class JsonSaveData<T> : SaveData<T> where T : class
{
    public JsonSaveData(string fileName) : base(fileName)
    {
    }

    public override string Serialize(T value) => JsonConvert.SerializeObject(value);
    public override T Deserialize(string value) => JsonConvert.DeserializeObject<T>(value);
}
