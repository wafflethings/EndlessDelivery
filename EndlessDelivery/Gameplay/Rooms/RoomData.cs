using UnityEngine;

namespace EndlessDelivery.Gameplay;

public class RoomData : ScriptableObject
{
    public string Name = "My Super Cool Room";
    public string Author = "Waffle";
    [Space(15)] public GameObject Prefab;
}
