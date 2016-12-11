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
	public List<string> pageTexts = new List<string>();
	RenderTexture renderTexture;
	bool levelLoading = false;

	void Start()
 	{
		Handheld.StopActivityIndicator();
		createRenderTexture();
		StartCoroutine( createPages() );
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
			book.bookPages[i] = pageSprite;
			if( i == 0 ) book.RightNext.sprite = pageSprite;
		}
		bookCanvas.gameObject.SetActive( true );
		pageCamera.enabled = false;
		pageCamera.targetTexture = null;
		RenderTexture.active = null;
		renderTexture = null;
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