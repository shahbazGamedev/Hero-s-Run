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
	int numberOfCharactersPerPage = 132;

	void Start()
 	{
		Handheld.StopActivityIndicator();
		createRenderTexture();
	}

	void createRenderTexture()
	{
		renderTexture = new RenderTexture(558, 864, 24);
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
		string story = jam.stories["Story 1"].text;
		//step 2 - populate pageTexts
		pageTexts.Clear();
		int pageCounter = 0;

		while ( story.Length > 0 )
		{
			if( story.Length > numberOfCharactersPerPage )
			{
				//if we are here, it means that what is left of the story does not fit in a single page.
				int index = findIndex( story.Substring( 0, numberOfCharactersPerPage ) );
				if( index == -1 ) break;
				pageTexts.Add( story.Substring( 0, index ).Trim() );
				story = story.Remove( 0, index ) ;
				pageCounter++;
			}
			else
			{
				//What is left of the story fits in a single page
				pageTexts.Add( story.Trim() );
				pageCounter++;
				break;
			}
		}
		book.setBookSize(pageCounter + 1 ); //plus one because of the cover

		//Add book cover
		book.addPageSprite( jam.covers["Cover 1"] );
		book.RightNext.sprite = jam.covers["Cover 1"];

		//step 3 - create pages
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