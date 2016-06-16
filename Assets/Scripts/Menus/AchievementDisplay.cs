using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchievementDisplay : MonoBehaviour {

	public static AchievementDisplay achievementDisplay = null;
	[Header("Achievement Display")]
	[Header("Message Panel")]
	public RectTransform messagePanel;
	Vector2 messagePanelDefaultPosition;
	public Image messageIcon;
	public Text messageText;
	[Header("Various portraits you can use")]
	public Image fairyPortrait;
	public Image darkQueenPortrait;
	public Image heroPortrait;

	Vector2 slideStartDest;
	Vector2 slideEndDest;
	const float SLIDE_DURATION = 0.5f;
	float waitDuration = 2.5f;
	
	// Use this for initialization
	void Awake () {
	
		achievementDisplay = this;
		messagePanelDefaultPosition = messagePanel.anchoredPosition;
		slideStartDest = new Vector2( 0, 0 );
		slideEndDest = new Vector2( messagePanel.rect.width, 0 );
	}

	//Used by the Game Center achievement system.
	//Achievement image can be null
	public void activateDisplay( string message, Texture2D image )
	{
		if( image != null )
		{
			Sprite	customImage = Sprite.Create( image, new Rect(0, 0, image.width, image.height ), new Vector2( 0.5f, 0.5f ) );
			activateDisplay( message, customImage, 2.5f );
		}
		else
		{
			//Use default Hero portrait since we have no achievement image
			activateDisplay( message, heroPortrait, 2.5f );
		}	
	}

	public void activateDisplayFairy( string message, float waitDuration )
	{
		activateDisplay( message, fairyPortrait, waitDuration );
	}

	public void activateDisplayDarkQueen( string message, float waitDuration )
	{
		activateDisplay( message, darkQueenPortrait, waitDuration );
	}

	void activateDisplay( string message, Sprite icon, float waitDuration )
	{
		this.waitDuration = waitDuration;
		messageText.text = message;
		messageIcon.sprite = icon;
		slideInMessage();
	}

	void activateDisplay( string message, Image icon, float waitDuration )
	{
		this.waitDuration = waitDuration;
		messageText.text = message;
		messageIcon = icon;
		slideInMessage();
	}

	void slideInMessage()
	{
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
		LeanTween.move( messagePanel, slideStartDest, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutMessage).setOnCompleteParam(gameObject);
	}
		
	void slideOutMessage()
	{
		//Wait a little before continuing to slide
		LeanTween.move( messagePanel, slideEndDest, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad).setDelay(waitDuration);
	}
	
	public void hideMessage()
	{
		LeanTween.cancelAll();
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState != GameState.Normal )
		{
			hideMessage();
		}
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			hideMessage();
		}
	}

}
