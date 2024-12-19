using System.Collections.Generic;
using AtlasLib.Saving;
using EndlessDelivery.Config;
using UnityEngine;

namespace EndlessDelivery.UI;

public class PresentColourUi : MonoBehaviour
{
    public static readonly List<SerializableColour> DefaultColours = [new(0, 0.91f, 1, 1), new(0.27f, 1, 0.27f, 1), new(1, 0.24f, 0.24f, 1), new(1, 0.88f, 0.24f, 1)];

    public void SliderChanged(int colourIndex, int rgbIndex, float value)
    {
        Plugin.Log.LogMessage($"Colour {colourIndex} [{rgbIndex}] = {value}");
        List<SerializableColour> colours = ConfigFile.Instance.Data.PresentColours;
        SerializableColour color = colours[colourIndex];
        color[rgbIndex] = value;
        colours[colourIndex] = color;
    }

    public void ResetColour(int colourIndex)
    {
        ConfigFile.Instance.Data.PresentColours[colourIndex] = DefaultColours[colourIndex];
    }
}
