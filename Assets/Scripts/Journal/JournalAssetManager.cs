using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class JournalAssetManager : MonoBehaviour {

	public static JournalAssetManager Instance;
	[Tooltip("The URL format for testing locally on a Mac is: file:///Users/regisgeoffrion/Documents/workspace/AssetBundles/covers. You need the 3 slashes after file:")]
    public string entriesBundleURL;
    public string coversBundleURL;
    public string storiesBundleURL;
    public int version;
	public Dictionary<string, Sprite> covers = new Dictionary<string, Sprite>();
	public Dictionary<string, TextAsset> stories = new Dictionary<string, TextAsset>();
	public TextAsset entriesJson;

	//For testing purposes
	public List<Sprite> testCovers = new List<Sprite>();
	string storyForTestPuposes = "{\"title\":\"The Fairy King's Treasure\",\"author\":\"Régis Geoffrion\"}Test! A demon materializes out of nowhere. When one of his two hoof touches the ground, a network of spidery cracks appears below, filled with flamelets. One of his black horns is broken, but the other is sharp as a spear. His eyes have a glint of evil. His sinister intent is clear. He is here for our treasure. How did he pass our protection spells, glyphs of protections and sigils? The demon laughed. He had appeared inside the Golden Vault. The treasure was within tantalizing reach. In the chest, a score feet away from him was a chest filled with enough fairy dust to resurrect an entire army. And my liege, King Merrylock, all dressed in purple and yellow, the most powerful mage of the Kingdom of Lum lied on a pile of shiny coins in a drunken stupor. It was up to me, Lily, to save the day. I was small, well tiny really, like all fairies. On a good day, I measured 1 foot. Okay, 11 inches to be precise if your counting. I had graduated from fairy school a full two weeks ago. Now graduating was a big event for me as I had failed my first year. And as all young graduates, I had been assigned to guard duty. Or like Silvestra said, to guard, the most precious treasure of the kingdom. It was boring, boring, boring. Nothing ever happened to it. Our liege, King Merrylock, was the most powerful mage of the Kingdom of Lum. The last person who tried to steal our treasure, one Balthazar More, had been transmogrified into a squiggly piglet.";

 	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
			initialise();
		}
	}

   void initialise()
	{
		GameManager.Instance.journalAssetManager = this;
		DontDestroyOnLoad( this );
		#if UNITY_EDITOR
        StartCoroutine (DownloadAndCacheEntries());
        StartCoroutine (DownloadAndCacheCovers());
        StartCoroutine (DownloadAndCacheStories());
		#endif
    }

   	IEnumerator DownloadAndCacheEntries ()
	{
		// Wait for the Caching system to be ready
		while (!Caching.ready)
            yield return null;

		// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        using(WWW www = WWW.LoadFromCacheOrDownload (entriesBundleURL, version))
		{
            yield return www;
            if (www.error != null)
			{
                throw new Exception( "WWW download had an error:" + www.error );
				yield break;
			}
            AssetBundle bundle = www.assetBundle;
			//Load the journal manifest (entries)
			AssetBundleRequest assetLoadAllTextRequest = bundle.LoadAllAssetsAsync<TextAsset>();
			yield return assetLoadAllTextRequest;
			UnityEngine.Object[] prefabs = assetLoadAllTextRequest.allAssets;
	
			entriesJson = Instantiate<TextAsset>(prefabs[0] as TextAsset );
			Debug.Log("JournalAssetManager-Entries found: " + entriesJson.text );

			// Unload the AssetBundles compressed contents to conserve memory
			bundle.Unload(false);

			GameObject.FindObjectOfType<JournalManager>().updateEntries( entriesJson.text );

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
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

	public string getStory( string storyFilename )
	{
		string story;
		#if UNITY_EDITOR
			//For the time being, the asset bundles that store the covers, stories, and the entries are on my Mac and not on the web.
			story = stories[storyFilename].text;
		#else
			story = storyForTestPuposes;
		#endif
		return story;
	}

	public Sprite getCover( string coverFilename )
	{
		Sprite cover;
		#if UNITY_EDITOR
			//For the time being, the asset bundles that store the covers, stories, and the entries are on my Mac and not on the web.
			cover = covers[coverFilename];
		#else
			int randomCover = UnityEngine.Random.Range(0,testCovers.Count);
			cover = testCovers[randomCover];
		#endif
		return cover;
	}


}

