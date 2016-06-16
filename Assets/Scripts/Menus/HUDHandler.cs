using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDHandler : MonoBehaviour {

	public static HUDHandler hudHandler = null;
	[Header("HUD Handler")]
 	public GameObject saveMeCanvas;
	public Text hudDebugInfo;	//Used to display FPS, player speed, etc.
	public Button pauseButton;
	[Header("Level Name Panel")]
	public RectTransform levelNamePanel;
	public Text levelNameText;	//Used to display FPS, player speed, etc.
	public Image levelIcon;
	[Header("Tap To Play")]
	//Tap to play button (the size of the canvas) with the Tap to play label
	//This is displayed when you start a level WHEN the game state changes to the MENU state.
	//Also see the waitForTapToPlay bool in LevelData
	public Button tapToPlayButton; 
	public Text tapToPlayText;
	//User Message is used to display the Go! message after resurrection and for some tutorial messages.
	//It appears in the center of the screen.
	[Header("User Message")]
	public Text userMessageText;

	//Number of coins collected
	static Rect coinIconRect;
	
	//Number of coins accumulated in coin series
	//There can be multiple displays at the same time.
	public Texture2D coinIconTexture;
	public GUIStyle coinAccumulatorStyle;
	private float coinAccumulatedDisplayDuration = 4f;
	GUIContent coinAccumulatorContent = new GUIContent( "" );
	static List<CoinDisplay> coinDisplayList = new List<CoinDisplay>();
	static float coinDisplayStartHeight = Screen.height * 0.4f;

	//FPS related
	string fps = "0";
	float fpsAccumulator = 0;
	//we do not want to update the fps value every frame
	int fpsFrameCounter = 0;
	int fpsWaitFrames = 30;
	
	public GUIStyle saveMeLevelInfoStyle;
	
	HUDSaveMe hudSaveMe;

	PlayerController playerController;


	// Use this for initialization
	void Awake ()
	{
		hudHandler = this;
		playerController = GetComponent<PlayerController>();

		//initialize for coin total
		coinIconRect = new Rect ( Screen.width * 0.6f, 10f, Screen.width * 0.09f, Screen.width * 0.09f );

		hudSaveMe = saveMeCanvas.GetComponent<HUDSaveMe>();

		//Adjust font sizes based on screen resolution
		PopupHandler.changeFontSizeBasedOnResolution( coinAccumulatorStyle );
		PopupHandler.changeFontSizeBasedOnResolution( saveMeLevelInfoStyle );

		tapToPlayText.text = LocalizationManager.Instance.getText("MENU_TAP_TO_PLAY");
		hudDebugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );

	}
	
	void Start()
	{
		//Display the name of the current level
		slideInLevelName();
	}
		
	// Update is called once per frame
	void OnGUI ()
	{
		showCoinTotal();
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateFPS();
		if( hudDebugInfo.gameObject.activeSelf ) hudDebugInfo.text = "FPS: " + fps + "-" + LevelManager.Instance.getNextLevelToComplete() + "-" + playerController.getCurrentTileName() + "-" + playerController.currentLane + "-" + PlayerController.getPlayerSpeed().ToString("N1");
	}
	
	void updateFPS()
	{
		//average out the fps over a number of frames
		fpsFrameCounter++;
		fpsAccumulator = fpsAccumulator + Time.deltaTime;
		if ( fpsFrameCounter == fpsWaitFrames )
		{
			fpsFrameCounter = 0;
			fps = Mathf.Ceil(fpsWaitFrames/fpsAccumulator).ToString();
			fpsAccumulator = 0;
		}
	}

	void startPlaying()
	{
		//If we are in the Menu state simply start running, but if we are in the OpeningSequence state
		//initiate the opening sequence instead.
		if (GameManager.Instance.getGameState() == GameState.Menu  )
		{
			//Disable the Tap to play button (and the associated Tap to Play label) since we do not need it anymore.
			tapToPlayButton.gameObject.SetActive( false );
			//Hide the level name panel in case it is showing
			hideLevelName();
			if( playerController.getCurrentTileType() == TileType.Opening )
			{
				SoundManager.playButtonClick();
				GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
				GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
				gem.playOpeningSequence();
			}
			else
			{
				SoundManager.playButtonClick();
				playerController.startRunning();
			}
		}
	}
	
	//Activates a horizontally centered text with a drop-shadow.
	//User Message is only displayed in the Normal game state.
	public void activateUserMessage( string text, float angle, float duration )
	{
		userMessageText.text = text;
		userMessageText.rectTransform.localRotation = Quaternion.Euler( 0, 0, angle );
		Invoke( "hideUserMessage", duration );
		userMessageText.gameObject.SetActive( true );
	}
		
	void hideUserMessage()
	{
		userMessageText.gameObject.SetActive( false );
	}

	void slideInLevelName()
	{
		levelNameText.text = LevelManager.Instance.getCurrentLevelName(); 
		LeanTween.move( levelNamePanel, new Vector2(0, -levelNamePanel.rect.height/2f), 0.5f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutLevelName).setOnCompleteParam(gameObject);
	}
		
	void slideOutLevelName()
	{
		//Wait a little before sliding back up
		LeanTween.move( levelNamePanel, new Vector2(0, levelNamePanel.rect.height/2f ), 0.5f ).setEase(LeanTweenType.easeOutQuad).setDelay(2.4f);
	}
	
	void hideLevelName()
	{
		LeanTween.cancelAll();
		levelNamePanel.anchoredPosition = new Vector2( 0, levelNamePanel.rect.height/2f );
	}
	
	public static Vector2 getCoinIconPos()
	{
		//Note: on the Mac, the perfect Y pos is: Screen.height - coinIconRect.yMax.
		//On the iPhone 5, however, the coins overshoot the icon slightly, hence the additional -coinIconRect.height/2f.
		Vector2 iconPos = new Vector2( Screen.width * 0.28f, Screen.height - coinIconRect.yMax - coinIconRect.height/2f );
		return iconPos;
	}
	
	public static void displayCoinTotal( int accumulatedCoins, Color colorCoin, bool isSequenceComplete )
	{
		if( PlayerStatsManager.Instance.getOwnsStarDoubler() ) accumulatedCoins = accumulatedCoins * 2;

		CoinDisplay coinDisplay = new CoinDisplay();
		coinDisplay.coinAccumulator = accumulatedCoins;
		coinDisplay.coinAccumulatedStartTime = Time.time;
		coinDisplay.isCoinSequenceComplete = isSequenceComplete;
		coinDisplay.coinColor = colorCoin;
		coinDisplay.coinAccumulatedIconRect = new Rect ( Screen.width * 0.7f, coinDisplayStartHeight - coinDisplayList.Count * 20, Screen.width * 0.17f, Screen.width * 0.17f );
		coinDisplayList.Add(coinDisplay);
	}
	
	void showCoinTotal()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal && coinDisplayList.Count > 0 )
		{
			for( int i=0; i < coinDisplayList.Count; i++ )
			{
				CoinDisplay cd = coinDisplayList[i];
				if( (Time.time - cd.coinAccumulatedStartTime) < coinAccumulatedDisplayDuration )
				{

					GUI.color = cd.coinColor;
					GUI.DrawTexture( cd.coinAccumulatedIconRect, coinIconTexture, ScaleMode.ScaleToFit, true );
					GUI.color = Color.white;
					
					//Draw the total number of accumulated coins in the center of the star
					coinAccumulatorContent.text = "+" + cd.coinAccumulator.ToString();
					Utilities.drawLabelWithDropShadow( cd.coinAccumulatedIconRect, coinAccumulatorContent, coinAccumulatorStyle );
					
					if( cd.isCoinSequenceComplete )
					{
						//Currently not used, but could be useful later
					}

				}
				else
				{
					//The time for this Display has expired. Remove this entry from the list.
					coinDisplayList.RemoveAt(i);
				}
			}
		}

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
		if( newState == GameState.Menu )
		{
			//Display the tap to play button
			tapToPlayButton.gameObject.SetActive( true );
		}
		else if( newState == GameState.SaveMe )
		{
			hudSaveMe.showSaveMeMenu();
		}

		if( newState == GameState.Normal )
		{
			hudDebugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
			pauseButton.gameObject.SetActive( true );
		}
		else
		{
			hudDebugInfo.gameObject.SetActive( false );
			pauseButton.gameObject.SetActive( false );
			userMessageText.gameObject.SetActive( false );
		}
	}

	public class CoinDisplay
	{
		public Rect coinAccumulatedIconRect;
		public int coinAccumulator = 0;
		public Color coinColor;
		public float coinAccumulatedStartTime = 0;
		public bool isCoinSequenceComplete = false;
	}


}

