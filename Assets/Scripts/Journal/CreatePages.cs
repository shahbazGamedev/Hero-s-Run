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
	public JournalAssetManager jam;
	public List<string> pageTexts = new List<string>();
	RenderTexture renderTexture;
	bool levelLoading = false;
	int numberOfCharactersPerPage = 272;
	public List<Sprite> testCovers = new List<Sprite>();

	void Start()
 	{
		Handheld.StopActivityIndicator();
		createRenderTexture();
		jam = GameManager.Instance.journalAssetManager;
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
		for( int i = 0; i < pageTexts.Count; i++ )
		{
	       	yield return new WaitForEndOfFrame();
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
		story = jam.stories["Story 1"].text;
		#else
		story = "The treasure. Wow! The demon materialized out of nowhere. When one of his two hoof touches the ground, a network of spidery cracks appears below, filled with flamelets. One of his black horns is broken, but the other is sharp as a spear. His eyes have a glint of evil. His sinister intent is clear. He is here for our treasure. How did he pass our protection spells, glyphs of protections and sigils? The demon laughed. He had appeared inside the Golden Vault. The treasure was within tantalizing reach. In the chest, a score feet away from him was a chest filled with enough fairy dust to resurrect an entire army. And my liege, King Merrylock, all dressed in purple and yellow, the most powerful mage of the Kingdom of Lum lied on a pile of shiny coins in a drunken stupor. It was up to me, Lily, to save the day. I was small, well tiny really, like all fairies. On a good day, I measured 1 foot. Okay, 11 inches to be precise if your counting. I had graduated from fairy school a full two weeks ago. Now graduating was a big event for me as I had failed my first year. And as all young graduates, I had been assigned to guard duty. Or like Silvestra said, to guard, the most precious treasure of the kingdom. It was boring, boring, boring. Nothing ever happened to it. Our liege, King Merrylock, was the most powerful mage of the Kingdom of Lum. The last person who tried to steal our treasure, one Balthazar More, had been transmogrified into a squiggly piglet.";
		#endif
 
		//step 2 - establish the number of visible characters that can fit in pageText.
		pageText.text = story;		
		Canvas.ForceUpdateCanvases(); 	//This is needed or else the TextGenerator values will not have been updated
		numberOfCharactersPerPage = pageText.cachedTextGenerator.characterCount;
		pageText.text = string.Empty;		

		//step 3 - populate pageTexts
		pageTexts.Clear();
		int pageCounter = 0;

		while ( story.Length > 0 )
		{
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

		//Add book cover
		int randomCover = UnityEngine.Random.Range( 1, 8 );
		#if UNITY_EDITOR
		//For the time being, the asset bundles that store the covers,stories, and entries are on my Mac and not on the web.
		string coverName = "Cover " + randomCover.ToString();
		book.addPageSprite( jam.covers[coverName] );
		book.RightNext.sprite = jam.covers[coverName];
		#else
		book.addPageSprite( testCovers[randomCover] );
		book.RightNext.sprite = testCovers[randomCover];
		#endif
	
		//step 4 - create pages
		StartCoroutine( createPages() );

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

}