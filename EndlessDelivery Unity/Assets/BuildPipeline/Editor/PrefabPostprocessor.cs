using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace BuildPipeline.Editor
{
    public class PrefabPostprocessor : AssetPostprocessor
    {
        // updating assets causes them to get postprocessed again, so we need to ignore them once. maybe a bit hacky but not terrible
        private static List<string> s_ignoreOnce = new List<string>();
        
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // call needs to be delayed as LoadPrefabContents crashes called from OnPostprocessAllAssets 
            // https://issuetracker.unity3d.com/issues/editor-freezes-when-prefabutility-dot-loadprefabcontents-is-called-in-assetpostprocessor-dot-onpostprocessallassets-for-a-moved-prefab
            EditorApplication.delayCall += () => ProcessPrefabs(importedAssets);
        }

        private static void ProcessPrefabs(string[] importedAssets)
        {
            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }

                if (s_ignoreOnce.Contains(path))
                {
                    s_ignoreOnce.Remove(path);
                    continue;
                }

                s_ignoreOnce.Add(path);
                ProcessPrefab(path);
            }
        }
        
        private static void ProcessPrefab(string assetPath)
        {
            UnityEngine.Debug.Log($"Processing prefab: {assetPath}");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            bool shouldSave = false;
            
            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                bool didReplace = BuiltInReplacementEditor.ReplaceRendererMaterial(renderer);
                shouldSave = shouldSave || didReplace;

                if (didReplace)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
            }

            if (shouldSave)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }
}