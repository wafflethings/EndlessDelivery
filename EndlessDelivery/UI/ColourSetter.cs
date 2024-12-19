using EndlessDelivery.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class ColourSetter : MonoBehaviour
{
    public int Colour;

    private void Start()
    {
        Color colour = ConfigFile.Instance.Data.GetColour(Colour);

        if (TryGetComponent(out Text text))
        {
            text.color = colour;
        }

        if (TryGetComponent(out TMP_Text text2))
        {
            text2.color = colour;
        }

        if (TryGetComponent(out Image image))
        {
            image.color = colour;
        }
    }
}
