using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Ultracrypt.Characters;
using Ultracrypt.Items;
using Ultracrypt.StatusEffects;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildPipeline.Editor
{
	public class BuildAddressables : MonoBehaviour
	{
		//EDIT THESE!!

		//AssetPathLocation needs to lead to a getter that returns the path where you store all your bundles in the mod.
		private const string AssetPathLocation = "{Ultracrypt.Assets.AssetManager.AssetPath}";

		//This is just the name of your mod.
		private const string MonoscriptBundleNaming = "ultracrypt";

		//Don't touch these unless you know what you're doing.
		private const string ResultPath = "Built Bundles";
		private const string DataFileName = "data.json";
		private const string WbpTemplateName = "WBP Assets";
		private const string CatalogPostfix = "wbp";
		private const string EmptyGroupName = "Empty Dont Delete";
		private const string EmptyAssetPath = "Assets/BuildPipeline/Assets/Empty.png";
		private static string[] s_commonGroupNames = { "Assets", "Game Prefabs", "Music", "Other" };

		private static AddressableAssetSettings Settings => AddressableAssetSettingsDefaultObject.Settings;

		[MenuItem("Addressable Build Pipeline/Debug Fast Build")]
		public static void FastBuildButton()
		{
			Build(BuildMode.Fast);
		}

		[MenuItem("Addressable Build Pipeline/Release Build")]
		public static void BuildButton()
		{
			Build(BuildMode.Full);
		}

		private static void Build(BuildMode buildMode)
		{
			ValidateAddressables();
			SetCorrectValuesForSettings();
			EnsureCustomTemplateExists();
			CreateEmptyGroup();

			if (!Directory.Exists(ResultPath))
			{
				Directory.CreateDirectory(ResultPath);
			}

			switch (buildMode)
			{
				case BuildMode.Full:
					BuildContent();
					break;
				
				case BuildMode.Fast:
					BuildContentFast();
					break;
			}

			ReplaceBuiltInWithEmpty();
			CreateDataFile();
		}

		private static void BuildContent()
		{
            if (Settings == null)
            {
                Debug.LogError("Settings is null!!");
                return;
            }

            AddMissingCommon(); //shouldnt be needed, but just in case,,
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new System.Exception(result.Error);
            }
        }

		// removes common groups (which take ages) but their needed assets are put in your bundles, increasing size
		private static void BuildContentFast()
		{
            if(Settings == null)
            {
                Debug.LogError("Settings is null!!");
                return;
            }

            Settings.groups.RemoveAll(group => group != null && s_commonGroupNames.Contains(group.name));
            RefreshGroups();

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError(result.Error);
                return;
            }
            
            AddMissingCommon();
		}

        // awful hack but yeah
        private static void AddMissingCommon()
        {
            string assetPath = "Assets/AddressableAssetsData/AssetGroups/{0}.asset";

            if (Settings.groups.All(group => group != null)) //.contains doesnt use the == overload, so .contains(null) is false when destroyed
            {
                Debug.Log("No null groups !!! Yippee!!");
            }

            Debug.Log("A group is missing !!!!");
            Settings.groups.RemoveAll(group => group == null);

            foreach (string commonGroup in s_commonGroupNames)
            {
                if (Settings.groups.Any(group => group != null && group.name == commonGroup))
                {
                    Debug.Log($"{commonGroup} exists");
                    continue;
                }

                Debug.Log($"Adding {commonGroup}!");
                Settings.groups.Add(AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(string.Format(assetPath, commonGroup)));
            }

            RefreshGroups();
        }
        
        private static void CreateDataFile()
        {
	        Dictionary<string, List<string>> dataInfo = new Dictionary<string, List<string>>();
	        AddData<ItemMetadata>(dataInfo, "Assets/Ultracrypt/Data/ItemMetadata");
            AddData<EffectMetadata>(dataInfo, "Assets/Ultracrypt/Data/EffectMetadata");
            AddData<CharacterMetadata>(dataInfo, "Assets/Ultracrypt/Data/CharacterMetadata");

	        using (StreamWriter writer = new StreamWriter(File.OpenWrite(Path.Combine(ResultPath, DataFileName))))
	        {
		        JsonSerializer serializer = new JsonSerializer();
		        serializer.Serialize(new JsonTextWriter(writer) { Formatting = Formatting.Indented }, dataInfo);
	        }
        }
		
        private static void AddData<T>(Dictionary<string, List<string>> dataInfo, string assetsPath) 
        {
	        List<string> value = Directory.GetFiles(assetsPath).Where((path) => !path.EndsWith(".meta")).Select((path) => path.Replace('\\', '/')).ToList();
	        dataInfo.Add(typeof(T).FullName, value);
        }

		private static void SetCorrectValuesForSettings()
		{
			Settings.profileSettings.CreateValue("ModBuildPath", ResultPath);
			Settings.profileSettings.CreateValue("ModLoadPath", AssetPathLocation);
			Settings.profileSettings.SetValue(Settings.activeProfileId, "ModBuildPath", ResultPath);
			Settings.profileSettings.SetValue(Settings.activeProfileId, "ModLoadPath", AssetPathLocation);

			Settings.IgnoreUnsupportedFilesInBuild = true;
			Settings.OverridePlayerVersion = CatalogPostfix;
			Settings.BuildRemoteCatalog = true;
			Settings.RemoteCatalogBuildPath.SetVariableByName(Settings, "ModBuildPath");
			Settings.RemoteCatalogLoadPath.SetVariableByName(Settings, "ModLoadPath");
			Settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Custom;
			Settings.MonoScriptBundleCustomNaming = MonoscriptBundleNaming;
			Settings.ShaderBundleNaming = ShaderBundleNaming.Custom;
			Settings.ShaderBundleCustomNaming = "shader";
		}

		private static void EnsureCustomTemplateExists()
		{
			foreach (ScriptableObject so in Settings.GroupTemplateObjects)
			{
				if ((bool)so && so.name == WbpTemplateName)
				{
					return;
				}
			}

			if (!Settings.CreateAndAddGroupTemplate(WbpTemplateName, "Assets for Waffle's Build Pipeline.", typeof(BundledAssetGroupSchema)))
			{
				Debug.LogError($"Failed to create the '{WbpTemplateName}' template, whar?");
				return;
			}

			AddressableAssetGroupTemplate wbpAssetsTemplate = Settings.GroupTemplateObjects[Settings.GroupTemplateObjects.Count - 1] as AddressableAssetGroupTemplate;

			if ((bool)wbpAssetsTemplate && wbpAssetsTemplate.Name != WbpTemplateName)
			{
				Debug.LogError($"Somehow got wrong template, this shouldn't be possible? [got {wbpAssetsTemplate.name}]");
				return;
			}

			BundledAssetGroupSchema groupSchema = wbpAssetsTemplate.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;

			if (!(bool)groupSchema)
			{
				Debug.LogError("Getting the schema from the template is null?");
				return;
			}

			SetCorrectValuesForSchema(groupSchema);
		}


        private static void RefreshGroups()
        {
            EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string assetPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AddressableAssetSettingsDefaultObject.Settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(assetPath);
        }

		// TundraEditor: Core/Editor/TundraInit.cs
		// thanks pitr i stole this completely ;3
		private static void ValidateAddressables(bool forceRewrite = false)
		{
			// TODO check the content
			var templatePostfix = ".template";
			var metaPostfix = ".meta";

			var assetPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
			var assetTemplatePath = assetPath + templatePostfix;

			var metaPath = assetPath + metaPostfix;
			var metaTemplatePath = assetPath + metaPostfix + templatePostfix;

			var valid = File.Exists(assetPath);

			if (!valid || forceRewrite)
			{
				Debug.Log($"Rewriting Addressables: {assetPath}");
				File.Copy(assetTemplatePath, assetPath, true);
				File.Copy(metaTemplatePath, metaPath, true);
				// Mark the asset database as dirty to force a refresh
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
				AddressableAssetSettingsDefaultObject.Settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(assetPath);
			}
		}

		private static void SetCorrectValuesForSchema(BundledAssetGroupSchema groupSchema)
		{
			groupSchema.IncludeInBuild = true;
			groupSchema.IncludeAddressInCatalog = true;
			groupSchema.BuildPath.SetVariableByName(Settings, "ModBuildPath");
			groupSchema.LoadPath.SetVariableByName(Settings, "ModLoadPath");
			groupSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
			groupSchema.UseAssetBundleCrcForCachedBundles = false;
			groupSchema.UseAssetBundleCrc = false;
		}

		private static void CreateEmptyGroup()
		{
			if (Settings.groups.Any(x => x.name == EmptyGroupName))
			{
				return;
			}

			AddressableAssetGroup group = Settings.CreateGroup(EmptyGroupName, false, false, false, null, typeof(BundledAssetGroupSchema));
			SetCorrectValuesForSchema(group.GetSchema<BundledAssetGroupSchema>());
			List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>
			{
				Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(EmptyAssetPath), group, false, false)
			};

			group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entries, false, true);
			Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entries, true, false);
		}

		private static void ReplaceBuiltInWithEmpty()
		{
			string emptyPath = Path.Combine(ResultPath, $"{EmptyGroupName.Replace(" ", "").ToLower()}_assets_all.bundle");
			string shaderPath = Path.Combine(ResultPath, $"{Settings.ShaderBundleCustomNaming}_unitybuiltinshaders.bundle");
			File.Delete(shaderPath);
			File.Move(emptyPath, shaderPath);
		}
	}
}