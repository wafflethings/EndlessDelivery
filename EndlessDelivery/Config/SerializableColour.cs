using System;
using UnityEngine;

namespace EndlessDelivery.Config;

public class SerializableColour
{
    public float R;
    public float G;
    public float B;
    public float A;

    public static SerializableColour FromUnity(Color color) => new SerializableColour(color.r, color.g, color.b, color.a);

    public Color ToUnity() => new Color(R, G, B, A);

    public SerializableColour(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public float this[int index]
    {
        get => index switch
            {
                0 => R,
                1 => G,
                2 => B,
                3 => A,
                _ => throw new ArgumentOutOfRangeException()
            };

        set
        {
            if (index == 0)
            {
                R = value;
                return;
            }

            if (index == 1)
            {
                G = value;
                return;
            }

            if (index == 2)
            {
                B = value;
                return;
            }

            if (index == 3)
            {
                A = value;
                return;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
