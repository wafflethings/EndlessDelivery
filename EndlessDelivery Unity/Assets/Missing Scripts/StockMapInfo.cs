using UnityEngine;

[DefaultExecutionOrder(-300)]
public class StockMapInfo : MapInfoBase
{
    private void Awake()
    {

    }
    
    public static StockMapInfo Instance;
    public SerializedActivityAssets assets;
}