using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AchievementDisplay : MonoBehaviour {

	public GUIStyle textStyle;

	static bool showDisplay = false;

	static Texture2D achievementBoxTextureT;
	static Texture2D achievementBoxTextureB;
	static Texture2D achievementImage;
	static Texture2D fairyImage;
	static Texture2D darkQueenImage;
	static Vector2 achievementBoxSize = new Vector2( Screen.width, 0.1f * Screen.height);
	static float margin = Screen.width * 0.05f;
	static LTRect achievementBoxRect = new LTRect( -Screen.width, 0.78f * Screen.height, achievementBoxSize.x, achievementBoxSize.y );
	static Hashtable options = new Hashtable();
	static Vector2 slideStartDest = new Vector2( 0, achievementBoxRect.rect.y );
	static Vector2 slideEndDest = new Vector2( Screen.width, achievementBoxRect.rect.y );
	static float delimiterHeight = 0.02f * Screen.height;
	static Rect topRect 	= new Rect( 0, 0, Screen.width, delimiterHeight);
	static Rect bottomRect 	= new Rect( 0, achievementBoxSize.y - delimiterHeight, Screen.width, delimiterHeight);
	static Vector2 iconSize = new Vector2( achievementBoxSize.y * 0.656f, achievementBoxSize.y * 0.656f );
	static Rect iconRect 	= new Rect( margin, (achievementBoxSize.y-iconSize.y)/2, iconSize.x, iconSize.y);
	static string achievementDescription;
	static float slideDuration = 0.6f;
	static float waitDuration = 2.5f;
	

	// Use this for initialization
	void Awake () {
	
		achievementBoxTextureT = Resources.Load("GUI/emerland") as Texture2D;
		achievementBoxTextureB = Resources.Load("GUI/alizarin") as Texture2D;
		//achievementImage is a back up in case the image provided from GameCenter is null
		achievementImage = Resources.Load("GUI/hero") as Texture2D;
		fairyImage = Resources.Load("GUI/Fairy_portrait02") as Texture2D;
		darkQueenImage = Resources.Load("GUI/Dark_Queen_portrait") as Texture2D;

		PopupHandler.changeFontSizeBasedOnResolution( textStyle );

	}

	void Start () {
		//Reset value
		options.Clear();
		options.Add("ease", LeanTweenType.easeOutQuad);
		options.Add("onComplete", "slideInEnded");
		options.Add("onCompleteTarget", gameObject );
		showDisplay = false;

	}

	public static void enableShowDisplay(  bool enable )
	{
		print("enableShowDisplay " + enable );
		showDisplay = enable;
	}

	public static void activateDisplay( string description, Texture2D image )
	{
		activateDisplay( description, image, 0.78f, 2.5f );
	}

	public static void activateDisplay( string description, Texture2D image, float boxHeight )
	{
		activateDisplay( description, image, boxHeight, 2.5f );
	}

	public static void activateDisplayFairy( string description, float boxHeight, float waitTime )
	{
		activateDisplay( description, fairyImage, boxHeight, waitTime );
	}

	public static void activateDisplayDarkQueen( string description, float boxHeight, float waitTime )
	{
		activateDisplay( description, darkQueenImage, boxHeight, waitTime );
	}

	public static void activateDisplay( string description, Texture2D image, float boxHeight, float waitTime )
	{
		waitDuration = waitTime;
		if( showDisplay )
		{
			Debug.LogWarning("AchievementDisplay-activateDisplay: busy right now. Achievement display is already active.");
			return;
		}
		//Reset rect position
		achievementBoxRect = new LTRect( -Screen.width, boxHeight * Screen.height, achievementBoxSize.x, achievementBoxSize.y );
		slideStartDest = new Vector2( 0, achievementBoxRect.rect.y );
		slideEndDest = new Vector2( Screen.width, achievementBoxRect.rect.y );

		options["onComplete"] = "slideInEnded";
		
		achievementImage = image;

		achievementDescription = description;
		LeanTween.move( achievementBoxRect, slideStartDest, slideDuration, options );
		enableShowDisplay( true );
	}

	void OnGUI ()
	{
		if( showDisplay )
		{
			displayAchievement();
		}
	}

	void displayAchievement()
	{
		GUI.BeginGroup( achievementBoxRect.rect );

		//draw a semi-transparent on top
		Color colPreviousGUIColor = GUI.color;
		GUI.color = new Color(colPreviousGUIColor.r, colPreviousGUIColor.g, colPreviousGUIColor.b, 0.5f);
		Rect test = new Rect( 0,0, Screen.width, achievementBoxSize.y );
		GUI.DrawTexture( test, achievementBoxTextureB );
		GUI.DrawTexture( topRect, achievementBoxTextureT );
		GUI.DrawTexture( bottomRect, achievementBoxTextureT );
		GUI.color = colPreviousGUIColor;
		GUI.DrawTexture( iconRect, achievementImage, ScaleMode.ScaleToFit, false );
		int maxTextWidth = (int) (achievementBoxSize.x - iconRect.xMax - margin - margin);
		textStyle.fixedWidth = maxTextWidth;
		GUIContent textContent = new GUIContent( achievementDescription );
		Rect textRect = GUILayoutUtility.GetRect( textContent, textStyle );
		textRect.x = iconRect.xMax + margin;
		textRect.y = (achievementBoxSize.y-textRect.height)/2;
		GUI.Label( textRect, textContent, textStyle );

		GUI.EndGroup();

	}

	void slideInEnded ()
	{
		options["onComplete"] = "slideOutEnded";
		StartCoroutine( slideDisplayOut() );
	}

	IEnumerator slideDisplayOut()
	{
		yield return new WaitForSeconds(waitDuration);
		LeanTween.move( achievementBoxRect, slideEndDest, slideDuration, options );
	}

	void slideOutEnded()
	{
		enableShowDisplay( false );
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState != GameState.Normal )
		{
			enableShowDisplay( false );
		}
	}

}
