using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Events;

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
		
		// Loop through all scene game objects
		GameObject[] sceneGameObjects = sceneVariant.GetRootGameObjects();
		foreach(GameObject sceneGameObject in sceneGameObjects){
			// Disconnect from prefab (if any)
			DisconnectPrefabInstances(sceneGameObject);
			
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
		GameObject prefabVariant = CopyAsset<GameObject>(prefabPath, prefabVariantPath);;
		
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
	
	private static void DisconnectPrefabInstances(GameObject gameObject){
		ExecuteOnGameObjectAndChildren(gameObject, go => {
			PrefabUtility.DisconnectPrefabInstance(go);
		});
	}
	
	// Replaces sprites in object (and children)
	private static void ReplaceSpritesWithVariants(GameObject gameObject, string variant){
		ExecuteOnGameObjectAndChildren(gameObject, go => {
			SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
			if (renderer != null){
				ReplaceSpriteWithVariant(renderer, variant);
			}
		});
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
		ExecuteOnGameObjectAndChildren(gameObject, go => {
			Animator animator = go.GetComponent<Animator>();
			if (animator != null){
				ReplaceAnimatorWithVariant(animator, variant);
			}
		});
	}

	// Replaces sprites in animations with variants
	private static bool ReplaceAnimatorWithVariant(Animator animator, string variant){
		bool clipVariantCreated = false;
		string animatorControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
		string animatorControllerVariantPath = GetAssetVariantPath(animatorControllerPath, variant);

		// If controller variant already exists -> use it (if we overwrite it, references to it will be lost)
		RuntimeAnimatorController animatorControllerVariant = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorControllerVariantPath);
		if (animatorControllerVariant != null){
			animator.runtimeAnimatorController = animatorControllerVariant;
			return false;
		}

		// Duplicate Controller and create new clips
		animatorControllerVariant = CopyAsset<RuntimeAnimatorController>(animatorControllerPath, animatorControllerVariantPath);

		if (animator.runtimeAnimatorController is AnimatorController){
			foreach(AnimatorControllerLayer layerVariant in ((AnimatorController)animatorControllerVariant).layers){
				foreach(ChildAnimatorState stateVariant in layerVariant.stateMachine.states){
					if (stateVariant.state.motion.GetType() == typeof(AnimationClip)){
						AnimationClip clip = (AnimationClip)stateVariant.state.motion;
						AnimationClip clipVariant = CreateAnimationClipVariant(clip, variant);

						if (clipVariant != null){
							clipVariantCreated = true;
							stateVariant.state.motion = clipVariant;
						}
					}
				}
			}
		}
		else if (animator.runtimeAnimatorController is AnimatorOverrideController){
			foreach(AnimationClipPair clipPair in ((AnimatorOverrideController)animatorControllerVariant).clips){
				AnimationClip clipVariant = CreateAnimationClipVariant(clipPair.overrideClip, variant);

				if (clipVariant != null){
					clipVariantCreated = true;
					((AnimatorOverrideController)animatorControllerVariant)[clipPair.originalClip.name] = clipVariant;
				}
			}
		} 

		// Use created animator controller, or delete it
		if (clipVariantCreated){
			animator.runtimeAnimatorController = animatorControllerVariant;
		}
		else{
			AssetDatabase.DeleteAsset(animatorControllerVariantPath);
		}

		return clipVariantCreated;
	}

		// Creates an animation clip variant if clip has references to sprites.
	// Returns the created animation clip (null if not created)
	private static AnimationClip CreateAnimationClipVariant(AnimationClip clip, string variant){
		bool spriteReplaced = false;;

		AnimationClip clipVariant = Object.Instantiate<AnimationClip>(clip);

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
						spriteReplaced = true;
					}
				}
			}
			AnimationUtility.SetObjectReferenceCurve(clipVariant, clipVariantCurveBinding, curveVariantKeyFrames);
		}

		if (spriteReplaced){
			return CreateOrReplaceAsset<AnimationClip>(clipVariant, GetAssetVariantPath(AssetDatabase.GetAssetPath(clip), variant));
		}

		return null;
	}

	private static T CopyAsset<T>(string path, string newPath) where T:Object{
		AssetDatabase.DeleteAsset(newPath);
		AssetDatabase.CopyAsset(path, newPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return AssetDatabase.LoadAssetAtPath<T>(newPath);
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
	
	private static void ExecuteOnGameObjectAndChildren(GameObject gameObject, UnityAction<GameObject> action){
		action(gameObject);
		
		for(int i=0; i<gameObject.transform.childCount; i++){
			ExecuteOnGameObjectAndChildren(gameObject.transform.GetChild(i).gameObject, action);
		}
	}

}
