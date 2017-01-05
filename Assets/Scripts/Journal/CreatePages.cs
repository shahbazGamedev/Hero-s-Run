using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatePages : MonoBehaviour {

	public Book book;
	public Camera pageCamera;
	public Canvas bookCanvas;
	public Text pageText;
	public Text titleText;
	public List<string> pageTexts = new List<string>();
	RenderTexture renderTexture;
	public EntryMetadata entryMetadata;

	void Start()
 	{
		Handheld.StopActivityIndicator();
		createRenderTexture();
	}

	void createRenderTexture()
	{
		renderTexture = new RenderTexture(553, 916, 24);
		renderTexture.antiAliasing = 4;
		renderTexture.format = RenderTextureFormat.Default;
		pageCamera.targetTexture = renderTexture;
	}

	public void generatePages( JournalData.JournalEntry selectedJournalEntry )
	{
		if( GameManager.Instance.journalAssetManager == null ) return;

		//step 1 - load appropriate story text
		string story = GameManager.Instance.journalAssetManager.getStory( selectedJournalEntry.storyName );
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
		Sprite cover = GameManager.Instance.journalAssetManager.getCover( selectedJournalEntry.coverName );
		book.addPageSprite( cover );
		book.RightNext.sprite = cover;

		//step 4 - create pages
		StartCoroutine( createPages() );

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

	string extractMetadata( string text )
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

	int findIndex( string story )
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
	int getNumberOfVisibleCharacters( string text )
	{
		pageText.text = text;		
		Canvas.ForceUpdateCanvases(); 	//This is needed or else the TextGenerator values will not have been updated
		return pageText.cachedTextGenerator.characterCount;
	}

	//Makes the font size of the first letter of the text bigger to give a fairy tale feel, but only if the first character is not a punctuation or line separator.
	string makeFirstLetterBigger( string text, int fontSize )
	{
		char[] firstLetterAsChar = text.ToCharArray(0,1);
		if( Char.IsSeparator( firstLetterAsChar[0] ) || Char.IsPunctuation( firstLetterAsChar[0] ) ) return text;
		string firstLetter = text.Substring(0,1);
		string firstLetterWithRichText = "<size=" + fontSize.ToString() + ">" + firstLetter + "</size>";
		text = text.Remove(0,1);
		text = text.Insert(0, firstLetterWithRichText );
		return text;
	}

	[System.Serializable]
	public class EntryMetadata
	{
		public string title = string.Empty;
		public string author = string.Empty;
	}

}