using UnityEngine;
using UnityEditor;

public class AssetVariantsMenu  {

	[MenuItem("Assets/Create Scene Variants", true)]
	static bool CreateSceneVariantsValidation(){
		return Selection.activeObject.GetType() == typeof(SceneAsset);
	}
	
	[MenuItem("Assets/Create Scene Variants", false, 10000)]
	static void CreateSceneVariants(){
		Object[] selectedScenes = Selection.GetFiltered(typeof(SceneAsset), SelectionMode.Assets);
		foreach(SceneAsset scene in selectedScenes){
			AssetVariants.CreateSceneVariant(scene, "sd");
		}
	}

	[MenuItem("Assets/Create Prefab Variants", true)] 
	static bool CreatePrefabVariantsValidation(){
		return Selection.activeObject.GetType() == typeof(GameObject);
	}
	
	[MenuItem("Assets/Create Prefab Variants", false, 10010)]
	static void CreatePrefabVariants(){
		Object[] selectedPrefabs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
		foreach (GameObject prefab in selectedPrefabs){
			AssetVariants.CreatePrefabVariant(prefab, "sd");
		}
	}
	
}
