using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem ("Assets/Build Asset Bundles")]
    static void BuildAllAssetBundles ()
    {
         BuildPipeline.BuildAssetBundles ("/Users/regisgeoffrion/Documents/workspace/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.iOS);
   }
}