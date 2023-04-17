using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleManager : MonoBehaviour
{
    
    [MenuItem("Assets/AssetBundleManager/LoadAssetBundle")]
    private static void LoadAssetBundle()
    {
        var bundlePath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        var bundle = AssetBundle.LoadFromFile(bundlePath);
        var prefabs = bundle.LoadAllAssets<GameObject>();
        foreach (var prefab in prefabs)
        {
            Instantiate(prefab);
        }
        bundle.Unload(false);
    }

    [MenuItem("Assets/AssetBundleManager/LoadAssetBundle", true)]
    private static bool LoadAssetBundleValidation()
    {
        return Selection.activeObject != null;
    }
    
    [MenuItem("Assets/AssetBundleManager/BuildAssetBundle")]
    private static void BuildAssetBundle()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory, 
            BuildAssetBundleOptions.None, 
            BuildTarget.StandaloneWindows64
            );

    }
}
