using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CreatePages : MonoBehaviour {

	public Book book;
	public Camera pageCamera;
	public Canvas bookCanvas;
	public Text pageText;
	public Text titleText;
	public List<string> pageTexts = new List<string>();
	RenderTexture renderTexture;
	bool levelLoading = false;
	public List<Sprite> testCovers = new List<Sprite>();
	string storyForTestPuposes = "{\"title\":\"The Fairy King's Treasure\",\"author\":\"Régis Geoffrion\"}Test! A demon materializes out of nowhere. When one of his two hoof touches the ground, a network of spidery cracks appears below, filled with flamelets. One of his black horns is broken, but the other is sharp as a spear. His eyes have a glint of evil. His sinister intent is clear. He is here for our treasure. How did he pass our protection spells, glyphs of protections and sigils? The demon laughed. He had appeared inside the Golden Vault. The treasure was within tantalizing reach. In the chest, a score feet away from him was a chest filled with enough fairy dust to resurrect an entire army. And my liege, King Merrylock, all dressed in purple and yellow, the most powerful mage of the Kingdom of Lum lied on a pile of shiny coins in a drunken stupor. It was up to me, Lily, to save the day. I was small, well tiny really, like all fairies. On a good day, I measured 1 foot. Okay, 11 inches to be precise if your counting. I had graduated from fairy school a full two weeks ago. Now graduating was a big event for me as I had failed my first year. And as all young graduates, I had been assigned to guard duty. Or like Silvestra said, to guard, the most precious treasure of the kingdom. It was boring, boring, boring. Nothing ever happened to it. Our liege, King Merrylock, was the most powerful mage of the Kingdom of Lum. The last person who tried to steal our treasure, one Balthazar More, had been transmogrified into a squiggly piglet.";
	public EntryMetadata entryMetadata;

	void Start()
 	{
		Handheld.StopActivityIndicator();
		createRenderTexture();
		generatePages();
	}

	void createRenderTexture()
	{
		renderTexture = new RenderTexture(553, 916, 24);
		renderTexture.antiAliasing = 4;
		renderTexture.format = RenderTextureFormat.Default;
		pageCamera.targetTexture = renderTexture;
	}

	// Use this for initialization
	IEnumerator createPages()
 	{
		Texture2D page;
		titleText.text = entryMetadata.title;
		for( int i = 0; i < pageTexts.Count; i++ )
		{
	       	yield return new WaitForEndOfFrame();
			if( i == 0 ) 
			{
				titleText.gameObject.SetActive( true );
			}
			else
			{
				titleText.gameObject.SetActive( false );
			}
			pageText.text = pageTexts[i];
			pageCamera.Render();
			RenderTexture.active = renderTexture;
			page = new Texture2D( renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false );
			page.ReadPixels( new Rect(0,0, page.width, page.height), 0, 0 );
			page.Apply();
			Sprite pageSprite = Sprite.Create( page, new Rect(0,0, page.width, page.height), new Vector2(0.5f, 0.5f) );
			pageSprite.name = "Page " + i.ToString();
			book.addPageSprite( pageSprite );
		}
		bookCanvas.gameObject.SetActive( true );
		pageCamera.enabled = false;
		pageCamera.targetTexture = null;
		RenderTexture.active = null;
		renderTexture = null;
	}

	public void generatePages()
	{
		//step 1 - load appropriate story text
		//string story = "This is a test.<Page Break>I will start jogging next week.<Page Break>I will get married in 2017. My wife and I will have regular, great, mutually satisfying sex including anal in 2017.<Page Break>My book will get published by a great, honest, competent, generous, clever, friendly book publisher and the sales will be phenomenal.<Page Break>Hero's Run is going to launch world wide and be amazingly popular and will monetise amazingly well. The game will be featured multiple times by Apple and Google. I will become even more rich!<Page Break>I will be invited as a speaker to Unite 2018.";
		string story;
		#if UNITY_EDITOR
		//For the time being, the asset bundles that store the covers and the stories are on my Mac and not on the web.
		//We test for journalAssetManager being null simply to be able to test directly in the journal scene without having to launch the game.
		if( GameManager.Instance.journalAssetManager != null )
		{
			 story = GameManager.Instance.journalAssetManager.stories["Story 1"].text;
		}
		else
		{
			story = storyForTestPuposes;
		}
		#else
			story = storyForTestPuposes;
		#endif
 
		//step 1b - extract the entry metadata such as the title and the author's name.
		story = extractMetadata( story );

		//step 1c - make the font size of the first letter of the story bigger to give a fairy tale feel.
		story = makeFirstLetterBigger( story, 36 );

		//step 2 - populate pageTexts
		pageTexts.Clear();
		int pageCounter = 0;
		int numberOfCharactersPerPage = 0;

		while ( story.Length > 0 )
		{
			numberOfCharactersPerPage = getNumberOfVisibleCharacters( story );

			if( story.Length > numberOfCharactersPerPage )
			{
				//if we are here, it means that what is left of the story does not fit in a single page.
				int index = findIndex( story.Substring( 0, numberOfCharactersPerPage ) );
				if( index == -1 ) break;
				pageTexts.Add( story.Substring( 0, index + 1 ).Trim() );
				story = story.Remove( 0, index + 1 ) ;
				pageCounter++;
			}
			else
			{
				//What is left of the story fits neatly in a single page
				pageTexts.Add( story.Trim() );
				pageCounter++;
				break;
			}
		}
		book.setBookSize(pageCounter + 1 ); //plus one because of the cover

		//step 3 - add book cover
		int randomCover = UnityEngine.Random.Range( 0, testCovers.Count );
		#if UNITY_EDITOR
		//For the time being, the asset bundles that store the covers,stories, and entries are on my Mac and not on the web.
		string coverName = "Cover " + randomCover.ToString();
		if( GameManager.Instance.journalAssetManager != null )
		{
			book.addPageSprite( GameManager.Instance.journalAssetManager.covers[coverName] );
			book.RightNext.sprite = GameManager.Instance.journalAssetManager.covers[coverName];
		}
		else
		{
			book.addPageSprite( testCovers[randomCover] );
			book.RightNext.sprite = testCovers[randomCover];
		}		
		#else
			book.addPageSprite( testCovers[randomCover] );
			book.RightNext.sprite = testCovers[randomCover];
		#endif
	
		//step 4 - create pages
		StartCoroutine( createPages() );

	}

	public string extractMetadata( string text )
	{
		int indexOpeningBracket = text.IndexOf("{");
		int indexClosingBracket = text.LastIndexOf("}");
		if( indexOpeningBracket == -1 || indexOpeningBracket == -1 )
		{
			Debug.LogWarning("Journal-extractMetadata: could not find metadata.");
			return string.Empty;
		}
		string json = text.Substring( indexOpeningBracket, indexClosingBracket + 1 );
		entryMetadata = JsonUtility.FromJson<EntryMetadata>(json);
		//Remove the json from the text
		text = text.Remove( indexOpeningBracket, indexClosingBracket + 1 );
		return text;
	}

	public int findIndex( string story )
	{
		char[] storyChar = story.ToCharArray();
		for (int i = storyChar.Length - 1; i >= 0; i--)
		{
			if( Char.IsSeparator( storyChar[i] ) || Char.IsPunctuation( storyChar[i] ) ) return i;
		}
		return -1;
	}

	//Used to establish the number of visible characters that can fit in pageText. Remember that
	//the size of a character is typically not constant in a font. A "1" is narrower than a "W" for example.
	public int getNumberOfVisibleCharacters( string text )
	{
		pageText.text = text;		
		Canvas.ForceUpdateCanvases(); 	//This is needed or else the TextGenerator values will not have been updated
		return pageText.cachedTextGenerator.characterCount;
	}

	//Makes the font size of the first letter of the text bigger to give a fairy tale feel, but only if the first character is not a punctuation or line separator.
	public string makeFirstLetterBigger( string text, int fontSize )
	{
		char[] firstLetterAsChar = text.ToCharArray(0,1);
		if( Char.IsSeparator( firstLetterAsChar[0] ) || Char.IsPunctuation( firstLetterAsChar[0] ) ) return text;
		string firstLetter = text.Substring(0,1);
		string firstLetterWithRichText = "<size=" + fontSize.ToString() + ">" + firstLetter + "</size>";
		text = text.Remove(0,1);
		text = text.Insert(0, firstLetterWithRichText );
		return text;
	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}

	[System.Serializable]
	public class EntryMetadata
	{
		public string title = string.Empty;
		public string author = string.Empty;
	}

}