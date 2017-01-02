using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreatePages : MonoBehaviour {

	public Book book;
	public Camera pageCamera;
	public Canvas bookCanvas;
	public Text pageText;
	public JournalAssetManager jam;
	public List<string> pageTexts = new List<string>();
	RenderTexture renderTexture;
	bool levelLoading = false;

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
			if( i == 0 ) book.RightNext.sprite = pageSprite;
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
		//step 2 - populate pageTexts by looking for page breaks.
		pageTexts.Clear();
		int pageCounter = 0;
		string pageBreakDelimiter = "<Page Break>";
		int pageBreakLength = pageBreakDelimiter.Length;
		while ( story.Length > 0 )
		{
			int pageBreakIndex = story.IndexOf( pageBreakDelimiter );
			if( pageBreakIndex >= 0 )
			{
				pageTexts.Add( story.Substring( 0, pageBreakIndex ) );
				story = story.Remove( 0, pageBreakIndex + pageBreakLength ) ;
			}
			else
			{
				pageTexts.Add( story.Substring( 0, story.Length ) );
				story = story.Remove( 0, story.Length ) ;
			}
			pageCounter++;
		}
		book.setBookSize(pageCounter);
		//step 3 - create pages
		StartCoroutine( createPages() );
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