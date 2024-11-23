using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BuildPipeline.Editor
{
	public abstract class BuiltInReplacementEditor : UnityEditor.Editor
	{
		private const string UnityBuiltInPath = "Resources/unity_builtin_extra";
		private const string BuiltInMaterialFolder = "Assets/BuildPipeline/BuiltInResources/";
		private const string BuiltInMaterialPath = BuiltInMaterialFolder + "{0}.asset";
		private UnityEditor.Editor _editorInstance;

		protected abstract string EditorName { get; }

		private void OnEnable()
		{
			// major hack, need to get the custom inspector for the renderer to call OnInspectorGUI but its private,,,
			Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
			Type typeEditor = assembly.GetType("UnityEditor." + EditorName);
			_editorInstance = CreateEditor(target, typeEditor);
		}

		// public override void OnInspectorGUI()
		// {
		// 	EditorGUI.BeginChangeCheck();
		// 	_editorInstance.OnInspectorGUI();
		//
		// 	if (EditorGUI.EndChangeCheck()) // i tried to use OnValidate but it is never called.. i love unity
		// 	{
		// 		ReplaceRendererMaterial(target as Renderer);
		// 	}
		// }

		public static bool ReplaceRendererMaterial(Renderer renderer)
		{
			bool hasReplaced = false;

			for (int i = 0; i < renderer.sharedMaterials.Length; i++)
			{
				Material replacement = GetReplacement(renderer.sharedMaterials[i]);

				if (replacement == null)
				{
					continue;
				}

				Material[] sharedMaterials = renderer.sharedMaterials;
				sharedMaterials[i] = replacement;
				renderer.sharedMaterials = sharedMaterials;
				hasReplaced = true;
			}

			return hasReplaced;
		}

		private static Material GetReplacement(Material material)
		{
            if (material == null || !AssetDatabase.GetAssetPath(material).StartsWith(UnityBuiltInPath))
            {
                return null;
            }

			string path = string.Format(BuiltInMaterialPath, material.name);

			if (!File.Exists(path))
			{
                Material duplicate = new Material(material);
                AssetDatabase.CreateAsset(duplicate, path);
			}

			return AssetDatabase.LoadAssetAtPath<Material>(path);
		}
	}

	// potentially some of my worst code ever? can't have several customeditor attributes on one class so this is necessary
	[CustomEditor(typeof(BillboardRenderer))]
	public class BillboardEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "BillboardRendererInspector";
	}

	[CustomEditor(typeof(CanvasRenderer))]
	public class CanvasEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "CanvasRendererEditor";
	}

	[CustomEditor(typeof(LineRenderer))]
	public class LineEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "LineRendererInspector";
	}

	[CustomEditor(typeof(MeshRenderer))]
	public class MeshEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "MeshRendererEditor";
	}

	[CustomEditor(typeof(SkinnedMeshRenderer))]
	public class SkinnedMeshEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "SkinnedMeshRendererEditor";
	}

	[CustomEditor(typeof(SpriteRenderer))]
	public class SpriteEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "SpriteRendererEditor";
	}

	[CustomEditor(typeof(TrailRenderer))]
	public class TrailEditor : BuiltInReplacementEditor
	{
		protected override string EditorName => "TrailRendererInspector";
	}
}
