using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem ("Assets/Build Asset Bundles")]
    static void BuildAllAssetBundles ()
    {
         BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.iOS);
   }
}