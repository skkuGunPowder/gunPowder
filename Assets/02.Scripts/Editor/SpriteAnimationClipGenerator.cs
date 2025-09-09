using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpriteAnimationClipGenerator
{
	[MenuItem("Tools/Sprite/Generate Clips From Selection %#g")]
	public static void GenerateFromSelection()
	{
		Object[] selection = Selection.objects;
		if (selection == null || selection.Length == 0)
		{
			EditorUtility.DisplayDialog("Generate Clips", "Select one or more folders, textures, or sprites in the Project window.", "OK");
			return;
		}

		Dictionary<string, List<Sprite>> folderToSprites = new Dictionary<string, List<Sprite>>();

		foreach (Object obj in selection)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(path))
				continue;

			if (Directory.Exists(path))
			{
				CollectSpritesGroupedByFolder(path, folderToSprites);
			}
			else
			{
				// Handle textures with multiple sprites or individual sprite assets
				List<Sprite> spritesInAsset = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList();
				if (spritesInAsset.Count == 0 && obj is Sprite singleSprite)
					spritesInAsset.Add(singleSprite);

				if (spritesInAsset.Count > 0)
				{
					string parentFolder = Path.GetDirectoryName(path).Replace('\\', '/');
					// If this is a sliced texture, group by texture name subfolder-like key to avoid mixing with other textures
					if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Texture2D))
						parentFolder = (parentFolder + "/" + Path.GetFileNameWithoutExtension(path)).Replace('\\', '/');

					if (!folderToSprites.TryGetValue(parentFolder, out List<Sprite> list))
					{
						list = new List<Sprite>();
						folderToSprites[parentFolder] = list;
					}
					list.AddRange(spritesInAsset);
				}
			}
		}

		int createdCount = 0;
		foreach (KeyValuePair<string, List<Sprite>> kvp in folderToSprites)
		{
			List<Sprite> sprites = kvp.Value.Where(s => s != null).OrderBy(s => s.name, new NaturalNameComparer()).ToList();
			if (sprites.Count == 0)
				continue;

			string destFolder = DetermineDestinationFolder(kvp.Key);
			EnsureFolderExists(destFolder);

			string clipName = Path.GetFileName(kvp.Key);
			string clipPath = AssetDatabase.GenerateUniqueAssetPath(destFolder + "/" + clipName + ".anim");

			AnimationClip clip = CreateClipFromSprites(sprites, 12f, false);
			AssetDatabase.CreateAsset(clip, clipPath);
			createdCount++;
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Generate Clips", $"Created {createdCount} AnimationClip(s).", "OK");
	}

	private static void CollectSpritesGroupedByFolder(string rootFolder, Dictionary<string, List<Sprite>> folderToSprites)
	{
		string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { rootFolder });
		foreach (string guid in spriteGuids)
		{
			string spritePath = AssetDatabase.GUIDToAssetPath(guid);
			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
			if (sprite == null)
				continue;

			string folder = Path.GetDirectoryName(spritePath).Replace('\\', '/');
			if (!folderToSprites.TryGetValue(folder, out List<Sprite> list))
			{
				list = new List<Sprite>();
				folderToSprites[folder] = list;
			}
			list.Add(sprite);
		}
	}

	private static string DetermineDestinationFolder(string groupKey)
	{
		return (groupKey + "/Animations").Replace('\\', '/');
	}

	private static void EnsureFolderExists(string folderPath)
	{
		folderPath = folderPath.Replace('\\', '/');
		if (AssetDatabase.IsValidFolder(folderPath))
			return;

		string[] parts = folderPath.Split('/');
		string current = parts[0];
		for (int i = 1; i < parts.Length; i++)
		{
			string next = current + "/" + parts[i];
			if (!AssetDatabase.IsValidFolder(next))
			{
				AssetDatabase.CreateFolder(current, parts[i]);
			}
			current = next;
		}
	}

	private static AnimationClip CreateClipFromSprites(List<Sprite> sprites, float frameRate, bool loop)
	{
		AnimationClip clip = new AnimationClip();
		clip.frameRate = frameRate;

		EditorCurveBinding binding = new EditorCurveBinding
		{
			path = string.Empty,
			type = typeof(SpriteRenderer),
			propertyName = "m_Sprite"
		};

		// Create keys so that each sprite holds for 1/frameRate seconds,
		// including an extra final key to retain the last frame until the end
		ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Count + 1];
		for (int i = 0; i < sprites.Count; i++)
		{
			keys[i] = new ObjectReferenceKeyframe
			{
				time = i / frameRate,
				value = sprites[i]
			};
		}
		// Extra terminal key
		keys[keys.Length - 1] = new ObjectReferenceKeyframe
		{
			time = sprites.Count / frameRate,
			value = sprites[sprites.Count - 1]
		};

		AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

		AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
		settings.loopTime = loop;
		settings.stopTime = sprites.Count / frameRate;
		AnimationUtility.SetAnimationClipSettings(clip, settings);

		return clip;
	}

	private sealed class NaturalNameComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			return EditorUtility.NaturalCompare(x, y);
		}
	}
}


