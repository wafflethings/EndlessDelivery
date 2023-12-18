using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EndlessDelivery.Utils
{
    public class PatchThis : Attribute
    {
        public static Dictionary<Type, PatchThis> AllPatches = new();

        public static void AddPatches()
        {
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<PatchThis>() != null))
            {
                Debug.Log($"Adding patch {t.Name}...");
                AllPatches.Add(t, t.GetCustomAttribute<PatchThis>());
            }

            PatchAll();
        }

        public static void PatchAll()
        {
            foreach (KeyValuePair<Type, PatchThis> kvp in AllPatches)
            {
                PatchThis pt = kvp.Value;
                Debug.Log($"Patching {kvp.Key.Name}...");
                pt._harmony.PatchAll(kvp.Key);
            }
        }

        private Harmony _harmony;
        public readonly string Name;

        public PatchThis(string harmonyName)
        {
            _harmony = new(harmonyName);
            Name = harmonyName;
        }
    }
}