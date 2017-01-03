using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class JournalAssetManager : MonoBehaviour {

	[Tooltip("The URL format for testing locally on a Mac is: file:///Users/regisgeoffrion/Documents/workspace/AssetBundles/covers. You need the 3 slashes after file:")]
    public string coversBundleURL;
    public string storiesBundleURL;
    public int version;
	public Dictionary<string, Sprite> covers = new Dictionary<string, Sprite>();
	public Dictionary<string, TextAsset> stories = new Dictionary<string, TextAsset>();

    void Awake()
	{
		GameManager.Instance.journalAssetManager = this;
		#if UNITY_EDITOR
        StartCoroutine (DownloadAndCacheCovers());
        StartCoroutine (DownloadAndCacheStories());
		#endif
    }

    IEnumerator DownloadAndCacheCovers ()
	{
        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

        //Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
		//Load story covers
        using(WWW www = WWW.LoadFromCacheOrDownload (coversBundleURL, version))
		{
            yield return www;
            if (www.error != null)
			{
                throw new Exception( "WWW download had an error:" + www.error );
				yield break;
			}
            AssetBundle bundle = www.assetBundle;

			//Load all cover sprites
			AssetBundleRequest assetLoadAllSpriteRequest = bundle.LoadAllAssetsAsync<Sprite>();
			yield return assetLoadAllSpriteRequest;
			UnityEngine.Object[] prefabs = assetLoadAllSpriteRequest.allAssets;
	
			Debug.Log("JournalAssetManager-Number of covers found: " + prefabs.Length);
			for( int i = 0; i < prefabs.Length; i++ )
			{
				covers.Add( prefabs[i].name, Instantiate<Sprite>(prefabs[i] as Sprite ) );

			}

           // Unload the AssetBundles compressed contents to conserve memory
           bundle.Unload(false);

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)

    }

    IEnumerator DownloadAndCacheStories ()
	{
        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

		// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        using(WWW www = WWW.LoadFromCacheOrDownload (storiesBundleURL + "." + getLanguage(), version))
		{
            yield return www;
            if (www.error != null)
			{
                throw new Exception( "WWW download had an error:" + www.error );
				yield break;
			}
            AssetBundle bundle = www.assetBundle;
			//Load all story texts
			AssetBundleRequest assetLoadAllTextRequest = bundle.LoadAllAssetsAsync<TextAsset>();
			yield return assetLoadAllTextRequest;
			UnityEngine.Object[] prefabs = assetLoadAllTextRequest.allAssets;
	
			Debug.Log("JournalAssetManager-Number of texts found:  " + prefabs.Length);
			for( int i = 0; i < prefabs.Length; i++ )
			{
				stories.Add( prefabs[i].name, Instantiate<TextAsset>(prefabs[i] as TextAsset ) );

			}

           // Unload the AssetBundles compressed contents to conserve memory
           bundle.Unload(false);

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
    }

   	string getLanguage()
	{
		string languageName; //needs to be in lower case because of asset bundle loading
		SystemLanguage language = Application.systemLanguage;
		switch (language)
		{
			case SystemLanguage.English:
				languageName = "english";
				break;
				
			case SystemLanguage.French:
				languageName = "french";
				break;
	
			default:
				//The language is not supported. Default to English.
				languageName = "english";
				break;
		}
		return languageName;
	}
}

