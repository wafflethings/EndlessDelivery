using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class ColourSetter : MonoBehaviour
{
    public static readonly Color[] DefaultColours = { new(0, 0.91f, 1), new(0.27f, 1, 0.27f), new(1, 0.24f, 0.24f), new(1, 0.88f, 0.24f) };

    public int Colour;

    private void Start()
    {
        if (TryGetComponent(out Text text))
        {
            text.color = DefaultColours[Colour];
        }

        if (TryGetComponent(out TMP_Text text2))
        {
            text2.color = DefaultColours[Colour];
        }
    }
}
