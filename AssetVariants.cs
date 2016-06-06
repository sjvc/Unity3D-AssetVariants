using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class AssetVariants {
	
	public static string VARIANT_PREFIX = "Variant-";

	// Creates a scene variant replacing sprites in objects and its animations
	public static string CreateSceneVariant(SceneAsset sceneAsset, string variant){
		// For each scene...
		Debug.Log("Generating " + sceneAsset.name + " " + variant + " variant");
		
		// Create scene variant
		string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
		string sceneVariantPath = GetAssetVariantPath(scenePath, variant);
		AssetDatabase.DeleteAsset(sceneVariantPath);
		AssetDatabase.CopyAsset(scenePath, sceneVariantPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		// Open scene
		Scene sceneVariant = EditorSceneManager.OpenScene(sceneVariantPath, OpenSceneMode.Additive);
		
		// Loop through all scene game objects with sprite renderer component
		GameObject[] sceneGameObjects = sceneVariant.GetRootGameObjects();
		foreach(GameObject sceneGameObject in sceneGameObjects){
			// Replace sprites
			ReplaceSpritesWithVariants(sceneGameObject, variant);
			
			// Create and replace Animation Controller
			ReplaceAnimatorsWithVariants(sceneGameObject, variant);
		}
		
		// Close scene
		EditorSceneManager.SaveScene(sceneVariant);
		EditorSceneManager.CloseScene(sceneVariant, true);
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("Done!");
		
		return sceneVariantPath;
	}

	// Creates a prefab variant replacing sprites in object (and children) and in animations
	public static string CreatePrefabVariant(GameObject prefab, string variant){
		Debug.Log("Generating " + prefab.name + " " + variant + " variant");
		
		// Create prefab variant
		string prefabPath = AssetDatabase.GetAssetPath(prefab);
		string prefabVariantPath = GetAssetVariantPath(prefabPath, variant);
		AssetDatabase.DeleteAsset(prefabVariantPath);
		AssetDatabase.CopyAsset(prefabPath, prefabVariantPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		GameObject prefabVariant = AssetDatabase.LoadAssetAtPath<GameObject>(prefabVariantPath);
		
		// Replace sprites
		ReplaceSpritesWithVariants(prefabVariant, variant);
		
		// Create and replace Animation Controller
		ReplaceAnimatorsWithVariants(prefabVariant, variant);
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("Done!");
		
		return prefabVariantPath;
	}	

	public static string GetVariantFolderName(string variant){
		return VARIANT_PREFIX + variant;
	}
	
	// Returns the path for the asset variant
	public static string GetAssetVariantPath(string path, string variant){
		if (!path.StartsWith("Assets/")){
			throw new System.Exception("Error, path should be inside Assets folder");
		}
		
		string variantPath = "Assets/" + GetVariantFolderName(variant) + "/" + path.Substring("Assets/".Length);
		
		CreateAssetDirs(variantPath);
		
		return AddSuffixToAssetName(variantPath, "-" + variant);
	}
	
	// Returns the path for the texture variant
	public static string GetTextureVariantPath(Texture2D texture, string variant){		
		string texturePath = AssetDatabase.GetAssetPath(texture);
		
		if (!texturePath.StartsWith("Assets/")){
			throw new System.Exception("Error, path should be inside Assets folder");
		}
		
		return GetAssetParentFolder(texturePath) + "/" + GetVariantFolderName(variant) + "/" + GetAssetFileName(texturePath);
	}

	// Creates inexistent directories in assetPath
	private static void CreateAssetDirs(string assetPath){
		string[] pathParts = assetPath.Split('/');
		string currentPath = "";
		string currentPathParent = "";
		
		for(int i=0; i<pathParts.Length-1; i++){
			currentPath = currentPathParent + (currentPathParent.Length > 0 ? "/" : "") + pathParts[i];
			
			if (!AssetDatabase.IsValidFolder(currentPath)){
				AssetDatabase.CreateFolder(currentPathParent, pathParts[i]);
			}
			
			currentPathParent = currentPath;
		}
	}
	
	private static string GetAssetParentFolder(string assetPath){
		if (assetPath.Contains("/")){
			return assetPath.Substring(0, assetPath.LastIndexOf("/"));
		}
		
		return "";
	}
	
	private static string GetAssetFileName(string assetPath){
		if (assetPath.Contains("/")){
			return assetPath.Substring(assetPath.LastIndexOf("/") + 1);
		}
		
		return assetPath;
	}

	private static string AddSuffixToAssetName(string assetPath, string suffix){
		string fileName = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
		if (fileName.Contains(".")){
			string fileNameWithoutExtension = fileName.Substring(0, fileName.LastIndexOf("."));
			string extension = fileName.Substring(fileName.LastIndexOf("."));
			fileName = fileNameWithoutExtension + suffix + extension;
		}
		else{
			fileName += suffix;
		}
		
		return assetPath.Substring(0, assetPath.LastIndexOf('/') + 1) + fileName;
	}

	// Returns the sprite variant of a given sprite. Null if not found.
	private static Sprite GetSpriteVariant(Sprite sprite, string variant){
		string texturePathVariant = GetTextureVariantPath(sprite.texture, variant);
		Object[] spriteVariants = AssetDatabase.LoadAllAssetsAtPath(texturePathVariant);
		foreach(Object spriteVariant in spriteVariants){
			if (spriteVariant.name == sprite.name){
				return (Sprite)spriteVariant;
			}
		}
		
		Debug.LogWarning("No sprite found for " + sprite.name + " in " + texturePathVariant);
		return null;
	}
	
	// Replaces sprites in object (and children)
	private static void ReplaceSpritesWithVariants(GameObject gameObject, string variant){
		SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
		if (renderer != null){
			ReplaceSpriteWithVariant(renderer, variant);
		}
		for(int i=0; i<gameObject.transform.childCount; i++){
			ReplaceSpritesWithVariants(gameObject.transform.GetChild(i).gameObject, variant);
		}
	}
	
	// Replaces sprites with variants
	private static bool ReplaceSpriteWithVariant(SpriteRenderer renderer, string variant){
		Sprite spriteVariant = GetSpriteVariant(renderer.sprite, variant);
		if (spriteVariant != null){
			renderer.sprite = spriteVariant;
			return true;
		}
		
		return false;
	}
	
	// Replaces sprites in object animations (and children)
	private static void ReplaceAnimatorsWithVariants(GameObject gameObject, string variant){
		Animator animator = gameObject.GetComponent<Animator>();
		if (animator != null){
			ReplaceAnimatorWithVariant(animator, variant);
		}
		for(int i=0; i<gameObject.transform.childCount; i++){
			ReplaceAnimatorsWithVariants(gameObject.transform.GetChild(i).gameObject, variant);
		}
	}
	
	// Replaces sprites in animations with variants
	private static bool ReplaceAnimatorWithVariant(Animator animator, string variant){
		RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
		
		if (animatorController == null){
			return false;
		}
		
		bool spriteReplacedInAnimator = false;
		
		// Create override animation controller, and replace clips
		AnimatorOverrideController animatorControllerVariant = new AnimatorOverrideController();
		animatorControllerVariant.runtimeAnimatorController = animatorController;
		foreach(AnimationClipPair clipPair in animatorControllerVariant.clips){
			bool spriteReplacedInClip = false;
			
			AnimationClip clip = clipPair.originalClip;
			
			// Create variant animation clip with no curves
			AnimationClip clipVariant = GameObject.Instantiate(clip);
			
			// For each curve...
			EditorCurveBinding[] clipVariantCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clipVariant);
			foreach(EditorCurveBinding clipVariantCurveBinding in clipVariantCurveBindings){
				ObjectReferenceKeyframe[] curveVariantKeyFrames = AnimationUtility.GetObjectReferenceCurve(clipVariant, clipVariantCurveBinding);
				// Replace sprites
				if (clipVariantCurveBinding.type == typeof(SpriteRenderer)){
					for(int i=0; i<curveVariantKeyFrames.Length; i++){
						Sprite sprite = (Sprite)curveVariantKeyFrames[i].value;
						Sprite spriteVariant = GetSpriteVariant(sprite, variant);
						if (spriteVariant != null){
							curveVariantKeyFrames[i].value = spriteVariant;
							spriteReplacedInClip = true;
						}
					}
				}
				AnimationUtility.SetObjectReferenceCurve(clipVariant, clipVariantCurveBinding, curveVariantKeyFrames);
			}
			
			if (spriteReplacedInClip){
				spriteReplacedInAnimator = true;
				
				// Save variant animation clip
				clipVariant = CreateOrReplaceAsset<AnimationClip>(clipVariant, GetAssetVariantPath(AssetDatabase.GetAssetPath(clip), variant));
				
				// Override clip in variant animation controller
				animatorControllerVariant[clip.name] = clipVariant;
			}
		}

		if (spriteReplacedInAnimator){
			// Save variant animation (override) controller
			animatorControllerVariant = CreateOrReplaceAsset<AnimatorOverrideController>(animatorControllerVariant, GetAssetVariantPath(AssetDatabase.GetAssetPath(animatorController), variant));
			animator.runtimeAnimatorController = animatorControllerVariant;
			return true;
		}
		
		return false;
	}
	
	private static T CreateOrReplaceAsset<T> (T asset, string path) where T:Object{
		 T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
		 
		 if (existingAsset == null){
			 AssetDatabase.CreateAsset(asset, path);
			 existingAsset = asset;
		 }
		 else{
			 EditorUtility.CopySerialized(asset, existingAsset);
		 }
		 
		 return existingAsset;
	}
}
