﻿using System.Collections.Generic;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Components;

public class RandomToggler : MonoBehaviour
{
    public List<GameObject> Objects;

    private void Start()
    {
        foreach (GameObject go in Objects)
        {
            go.SetActive(false);
        }

        Objects.Pick().SetActive(true);
    }
}
