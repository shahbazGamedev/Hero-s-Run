using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HUDHandler : MonoBehaviour {

	public static HUDHandler hudHandler = null;
	[Header("HUD Handler")]
 	public GameObject saveMeCanvas;
	public Text hudDebugInfo;	//Used to display FPS, player speed, etc.
	public Button pauseButton;
	[Header("Level Name Panel")]
	public RectTransform episodeNamePanel;
	public Text episodeNameText;
	[Header("Tap To Play")]
	//Tap to play button (the size of the canvas) with the Tap to play label
	//This is displayed when you start a level WHEN the game state changes to the MENU state.
	//Also see the waitForTapToPlay bool in LevelData
	public Button tapToPlayButton; 
	public Text tapToPlayText;
	//User Message is used to display the Go! message after resurrection.
	//It appears in the center of the screen.
	[Header("User Message")]
	public Text userMessageText;
	[Header("Coin and Treasure Key Display")]
	public RectTransform hudCanvas;
	public GameObject coinPrefab;
	public GameObject treasurePrefab;
	public GameObject restartFromCheckpointPanel; //Used to inform the player that he is restarting from a checkpoint and not from the begining
	public Text restartFromCheckpointText; 
	[Header("Used for fading effects")]
	public Image fadeImage;
	public CanvasGroup canvasGroup;
	System.Action onFinish;
	[Header("Journal")]
	public GameObject journalCanvas;

	//Used to track the items picked up by the player such as Coins and Treasure Keys. Multiple icons can be displayed at the same time with an offset.
	List<PickupDisplay> pickupDisplayList = new List<PickupDisplay>();
	const float PICKUP_DISPLAY_DURATION = 4f;

	//FPS related
	string fps = "0";
	float fpsAccumulator = 0;
	//we do not want to update the fps value every frame
	int fpsFrameCounter = 0;
	int fpsWaitFrames = 30;
	
	HUDSaveMe hudSaveMe;
	PlayerController playerController;
	Rect coinIconRect; //Used to position the coins collected at the top of the HUD

	// Use this for initialization
	void Awake ()
	{
		hudHandler = this;
		hudSaveMe = saveMeCanvas.GetComponent<HUDSaveMe>();
		tapToPlayText.text = LocalizationManager.Instance.getText("MENU_TAP_TO_PLAY");
		restartFromCheckpointText.text = LocalizationManager.Instance.getText("MENU_RESTART_FROM_CHECKPOINT");
		hudDebugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		coinIconRect = new Rect ( Screen.width * 0.6f, 10f, Screen.width * 0.09f, Screen.width * 0.09f );
	}
	
	void Start()
	{
		if( !GameManager.Instance.isMultiplayer() )
		{
			//Display the name of the current level
			slideInEpisodeName();
		}
		else
		{	//There is no tap to play button in multiplayer. There is a countdown instead.
			tapToPlayButton.gameObject.SetActive( false );
		}

	}
		
	// Update is called once per frame
	void Update ()
	{
		updateFPS();
		if( hudDebugInfo.gameObject.activeSelf ) hudDebugInfo.text = " FPS: " + fps + "-" + LevelManager.Instance.getNextEpisodeToComplete() + "-" + playerController.getCurrentTileName() + "-" + PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + "-" + PlayerController.getPlayerSpeed().ToString("N1");
		managePickUps();
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.U) ) 
		{
			fadeEffect( true );
		}
		else if ( Input.GetKeyDown (KeyCode.I) ) 
		{
			fadeEffect( false );
		}
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

	//Start playing is called when the tapToPlayButton is pressed
	public void startPlaying()
	{
		//Disable the Tap to play button (and the associated Tap to Play label) since we do not need it anymore.
		tapToPlayButton.gameObject.SetActive( false );
		//Hide the text that might be showing telling the player he is restarting from a checkpoint
		restartFromCheckpointPanel.SetActive( false );

		//Hide the level name panel in case it is showing
		hideLevelName();
		if( playerController.getCurrentTileType() == TileType.Opening )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
			GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
			gem.playOpeningSequence();
		}
		else
		{
			UISoundManager.uiSoundManager.playButtonClick();
			playerController.startRunning();
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

	void slideInEpisodeName()
	{
		//EPISODE_NAME_X is the text ID to use to get the localised episode name where X is the episode name indexed starting at 1.
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string episodeNumberString = (episodeNumber + 1).ToString();
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + episodeNumberString );
		LeanTween.move( episodeNamePanel, new Vector2(0, -episodeNamePanel.rect.height/2f), 0.5f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutEpisodeName).setOnCompleteParam(gameObject);
	}
		
	void slideOutEpisodeName()
	{
		//Wait a little before sliding back up
		LeanTween.move( episodeNamePanel, new Vector2(0, episodeNamePanel.rect.height/2f ), 0.5f ).setEase(LeanTweenType.easeOutQuad).setDelay(2.5f);
	}
	
	void hideLevelName()
	{
		LeanTween.cancel(gameObject);
		episodeNamePanel.anchoredPosition = new Vector2( 0, episodeNamePanel.rect.height/2f );
	}

	public Vector2 getCoinIconPos()
	{
		//Note: on the Mac, the perfect Y pos is: Screen.height - coinIconRect.yMax.
		//On the iPhone 5, however, the coins overshoot the icon slightly, hence the additional -coinIconRect.height/2f.
		Vector2 iconPos = new Vector2( Screen.width * 0.28f, Screen.height - coinIconRect.yMax - coinIconRect.height/2f );
		return iconPos;
	}
	
	public void displayCoinPickup( int quantity )
	{
		if( PlayerStatsManager.Instance.getOwnsCoinDoubler() ) quantity = quantity * 2;

		GameObject go = (GameObject)Instantiate(coinPrefab);
		go.transform.SetParent( hudCanvas.transform, false );
		Text quantityText = go.GetComponentInChildren<Text>();
		quantityText.text = "+" + quantity.ToString();
		RectTransform rt = go.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2( rt.anchoredPosition.x, rt.anchoredPosition.y - ( pickupDisplayList.Count * rt.rect.height * 0.4f ) );
		go.SetActive( true );
		PickupDisplay pickupDisplay = new PickupDisplay();
		pickupDisplay.startTime = Time.time;
		pickupDisplay.objectPickedUp = go;
		pickupDisplayList.Add(pickupDisplay);
	}
	
	public void displayTreasureKeyPickup()
	{
		GameObject go = (GameObject)Instantiate(treasurePrefab);
		go.transform.SetParent( hudCanvas.transform, false );
		RectTransform rt = go.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2( rt.anchoredPosition.x, rt.anchoredPosition.y - ( pickupDisplayList.Count * rt.rect.height * 0.4f ) );
		go.SetActive( true );
		PickupDisplay pickupDisplay = new PickupDisplay();
		pickupDisplay.startTime = Time.time;
		pickupDisplay.objectPickedUp = go;
		pickupDisplayList.Add(pickupDisplay);
	}

	void managePickUps()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal && pickupDisplayList.Count > 0 )
		{
			for( int i=0; i < pickupDisplayList.Count; i++ )
			{
				PickupDisplay pd = pickupDisplayList[i];
				if( (Time.time - pd.startTime) > PICKUP_DISPLAY_DURATION )
				{
					//The time for this Display has expired. Remove this entry from the list.
					pickupDisplayList.RemoveAt(i);
					GameObject.Destroy( pd.objectPickedUp );
				}
			}
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
		PlayerController.localPlayerCreated += LocalPlayerCreated;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
		PlayerController.localPlayerCreated -= LocalPlayerCreated;
	}

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			pauseButton.gameObject.SetActive( false );
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Menu )
		{
			//Display the tap to play button
			tapToPlayButton.gameObject.SetActive( true );
			//If the player is restarting from a checkpoint, tell him
			if( LevelManager.Instance.getNumberOfCheckpointsPassed() > 0 )
			{
				restartFromCheckpointPanel.SetActive( true );
			}
		}
		else if( newState == GameState.SaveMe )
		{
			hudSaveMe.showSaveMeMenu();
		}

		if( newState == GameState.Normal )
		{
			hudDebugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
			pauseButton.gameObject.SetActive( true );
			journalCanvas.gameObject.SetActive( true );
		}
		else
		{
			journalCanvas.gameObject.SetActive( false );
			hudDebugInfo.gameObject.SetActive( false );
			pauseButton.gameObject.SetActive( false );
			userMessageText.gameObject.SetActive( false );
			destroyAllPickupsDisplayed();
		}
	}

	void LocalPlayerCreated( Transform playerTransform, PlayerController playerController )
	{
		this.playerController = playerController;
	}

	void destroyAllPickupsDisplayed()
	{
		for( int i=0; i < pickupDisplayList.Count; i++ )
		{
			PickupDisplay pd = pickupDisplayList[i];
			GameObject.Destroy( pd.objectPickedUp );
		}
		pickupDisplayList.Clear();
	}

	public class PickupDisplay
	{
		public GameObject objectPickedUp;
		public float startTime = 0;
	}

	public void fadeEffect( bool fadeInEffect, System.Action onFinish = null )
	{
		this.onFinish = onFinish;
		if( fadeInEffect )
		{
			//Fade-out UI elements like the pause button
			LeanTween.alphaCanvas( canvasGroup, 0, 2f );
			//Fade-in the white overlay
			LeanTween.color( fadeImage.GetComponent<RectTransform>(), new Color( 1f, 1f, 1f, 1f ), 5f ).setOnComplete(fadeCompleted).setOnCompleteParam(gameObject);;
		}
		else
		{
			//Fade-in UI elements like the pause button
			LeanTween.alphaCanvas( canvasGroup, 1f, 2f );
			//Fade-out the white overlay
			LeanTween.color( fadeImage.GetComponent<RectTransform>(), new Color( 1f, 1f, 1f, 0f ), 5f ).setOnComplete(fadeCompleted).setOnCompleteParam(gameObject);;
		}
	}

	void fadeCompleted()
	{
		if( onFinish != null ) onFinish.Invoke();
	}
}

