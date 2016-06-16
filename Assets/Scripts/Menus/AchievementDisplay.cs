using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AchievementDisplay : MonoBehaviour {

	public static AchievementDisplay achievementDisplay = null;
	[Header("Achievement Display")]
	[Header("Message Panel")]
	public RectTransform messagePanel;
	Vector2 messagePanelDefaultPosition;
	public Image messageIcon;
	public Text messageText;
	public Image fairyPortrait;
	public Image darkQueenPortrait;
	public Image heroPortrait;

	Vector2 slideStartDest;
	Vector2 slideEndDest;
	bool showDisplay = false;
	float slideDuration = 0.6f;
	float waitDuration = 2.5f;

	public GUIStyle textStyle;


	Texture2D achievementBoxTextureT;
	Texture2D achievementBoxTextureB;
	Texture2D achievementImage;
	Texture2D fairyImage;
	Texture2D darkQueenImage;
	Vector2 achievementBoxSize = new Vector2( Screen.width, 0.1f * Screen.height);
	float margin = Screen.width * 0.05f;
	LTRect achievementBoxRect;
	float delimiterHeight;
	Rect topRect;
	Rect bottomRect;
	Vector2 iconSize;
	Rect iconRect;
	string achievementDescription;
	
	// Use this for initialization
	void Awake () {
	
		achievementDisplay = this;
		messagePanelDefaultPosition = messagePanel.anchoredPosition;
		achievementBoxRect = new LTRect( -Screen.width, 0.78f * Screen.height, achievementBoxSize.x, achievementBoxSize.y );
		slideStartDest = new Vector2( 0, 0 );
		slideEndDest = new Vector2( messagePanel.rect.width, 0 );
		delimiterHeight = 0.02f * Screen.height;
		topRect 	= new Rect( 0, 0, Screen.width, delimiterHeight);
		bottomRect 	= new Rect( 0, achievementBoxSize.y - delimiterHeight, Screen.width, delimiterHeight);
		iconSize = new Vector2( achievementBoxSize.y * 0.656f, achievementBoxSize.y * 0.656f );
		iconRect 	= new Rect( margin, (achievementBoxSize.y-iconSize.y)/2, iconSize.x, iconSize.y);

		achievementBoxTextureT = Resources.Load("GUI/emerland") as Texture2D;
		achievementBoxTextureB = Resources.Load("GUI/alizarin") as Texture2D;
		//achievementImage is a back up in case the image provided from GameCenter is null
		achievementImage = Resources.Load("GUI/hero") as Texture2D;
		fairyImage = Resources.Load("GUI/Fairy_portrait02") as Texture2D;
		darkQueenImage = Resources.Load("GUI/Dark_Queen_portrait") as Texture2D;

		PopupHandler.changeFontSizeBasedOnResolution( textStyle );

	}

	void Start () {
		showDisplay = false;

	}

	public void enableShowDisplay(  bool enable )
	{
		print("enableShowDisplay " + enable );
		showDisplay = enable;
	}

	public void activateDisplay( string description, Texture2D image )
	{
		activateDisplay( description, image, 0.78f, 2.5f );
	}

	public void activateDisplay( string description, Texture2D image, float boxHeight )
	{
		activateDisplay( description, image, boxHeight, 2.5f );
	}

	public void activateDisplayFairy( string description, float boxHeight, float waitTime )
	{
		messageText.text = description;
		messageIcon = fairyPortrait;
		slideInMessage();
		//activateDisplay( description, fairyImage, boxHeight, waitTime );
	}

	public void activateDisplayDarkQueen( string description, float boxHeight, float waitTime )
	{
		activateDisplay( description, darkQueenImage, boxHeight, waitTime );
	}

	public void activateDisplay( string description, Texture2D image, float boxHeight, float waitTime )
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
		
		achievementImage = image;

		achievementDescription = description;
		LeanTween.move( achievementBoxRect, slideStartDest, slideDuration).setEase(LeanTweenType.easeOutQuad).setOnComplete( slideInEnded ).setOnCompleteParam(gameObject);
		enableShowDisplay( true );
	}

	void slideInMessage()
	{
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
		LeanTween.move( messagePanel, slideStartDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutMessage).setOnCompleteParam(gameObject);
	}
		
	void slideOutMessage()
	{
		//Wait a little before continuing to slide
		LeanTween.move( messagePanel, slideEndDest, 0.5f ).setEase(LeanTweenType.easeOutQuad).setDelay(waitDuration);
	}
	
	void hideMessage()
	{
		LeanTween.cancelAll();
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
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
		StartCoroutine( slideDisplayOut() );
	}

	IEnumerator slideDisplayOut()
	{
		yield return new WaitForSeconds(waitDuration);
		LeanTween.move( achievementBoxRect, slideEndDest, slideDuration ).setEase(LeanTweenType.easeOutQuad).setOnComplete( slideOutEnded ).setOnCompleteParam(gameObject);
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
