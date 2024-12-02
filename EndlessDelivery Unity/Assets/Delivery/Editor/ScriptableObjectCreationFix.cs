using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EndlessDelivery.Editor
{
    public static class ScriptableObjectCreationFix
    {
        private const string ButtonPath = "Assets/Create/EndlessDelivery";
        private const string ModAssemblyName = "EndlessDelivery";

        private static readonly Type s_menuType = GetAssemblyByName("UnityEditor").GetTypeByName("Menu");
        private static readonly MethodInfo s_addMenuItem = s_menuType.GetMethod("AddMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo s_removeMenuItem = s_menuType.GetMethod("RemoveMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo s_menuItemExists = s_menuType.GetMethod("MenuItemExists", BindingFlags.Static | BindingFlags.NonPublic);

        public static void RemoveMenuItem(string name) => s_removeMenuItem.Invoke(null, new object[] { name });
        public static bool MenuItemExists(string menuPath) => (bool)s_menuItemExists.Invoke(null, new object[] { menuPath });
        public static void AddMenuItem(string name, Action execute) => s_addMenuItem.Invoke(null, new object[] { name, null, null, -1, execute, null });

        private static System.Reflection.Assembly GetAssemblyByName(string assembly)
        {
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Debug.Log(asm.FullName);
            }

            return AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.FullName.StartsWith(assembly + ","));
        }

        private static Type GetTypeByName(this System.Reflection.Assembly assembly, string name)
        {
            return assembly.GetTypes().First(type => type.Name == name);
        }

        private static bool TryGetActiveFolderPath(out string path)
        {
            MethodInfo tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = { null };
            bool found = (bool)tryGetActiveFolderPath.Invoke(null, args);
            path = (string)args[0];

            return found;
        }

        // https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters?page=2
        private static string SpaceCapitals(this string arg) => new string(arg.Aggregate(new List<Char>(), (accum, x) =>
        {
            if (Char.IsUpper(x) && accum.Any() && accum.Last() != ' ' && !Char.IsUpper(accum.Last()))
            {
                accum.Add(' ');
            }

            accum.Add(x);

            return accum;
        }).ToArray());

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.delayCall += CreateMenu;
        }

        private static string FormPathWithSuffix(string path, int suffix) => suffix == 0 ? path : path.Replace(".asset", string.Empty) + " " + suffix + ".asset";

        public static IEnumerable<Type> GetLoadableTypes(this System.Reflection.Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        private static void CreateMenu()
        {
            Type soType = typeof(ScriptableObject);

            foreach (Type type in GetAssemblyByName(ModAssemblyName).GetLoadableTypes())
            {
                Debug.Log(type.Name);
                if (!soType.IsAssignableFrom(type))
                {
                    continue;
                }

                AddMenuItem(ButtonPath + "/" + type.Name, () =>
                {
                    if (TryGetActiveFolderPath(out string path))
                    {
                        string assetPath = Path.Combine(path, $"New {SpaceCapitals(type.Name)}.asset");
                        int suffix = 0;

                        while (File.Exists(FormPathWithSuffix(assetPath, suffix)))
                        {
                            suffix++;
                        }

                        string finalPath = FormPathWithSuffix(assetPath, suffix);
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(type), finalPath);
                    }
                });
            }
        }
    }
}
