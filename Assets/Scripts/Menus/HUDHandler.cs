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
	public Text episodeNameText;	//Used to display FPS, player speed, etc.
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
	[Header("Star and Treasure Key Display")]
	public RectTransform hudCanvas;
	public GameObject starPrefab;
	public GameObject treasurePrefab;
	
	//Used to track the items picked up by the player such as Stars and Treasure Keys. Multiple icons can be displayed at the same time with an offset.
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
	Rect coinIconRect; //Used to position the stars collected at the top of the HUD

	// Use this for initialization
	void Awake ()
	{
		hudHandler = this;
		playerController = GetComponent<PlayerController>();
		hudSaveMe = saveMeCanvas.GetComponent<HUDSaveMe>();
		tapToPlayText.text = LocalizationManager.Instance.getText("MENU_TAP_TO_PLAY");
		hudDebugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		coinIconRect = new Rect ( Screen.width * 0.6f, 10f, Screen.width * 0.09f, Screen.width * 0.09f );
	}
	
	void Start()
	{
		//Display the name of the current level
		slideInEpisodeName();
	}
		
	// Update is called once per frame
	void Update ()
	{
		updateFPS();
		if( hudDebugInfo.gameObject.activeSelf ) hudDebugInfo.text = "Troll: " + playerController.trollController.didPlayerStumblePreviously() + " FPS: " + fps + "-" + LevelManager.Instance.getNextEpisodeToComplete() + "-" + playerController.getCurrentTileName() + "-" + PlayerStatsManager.Instance.getTimesPlayerRevivedInLevel() + "-" + PlayerController.getPlayerSpeed().ToString("N1");
		managePickUps();
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
				SoundManager.soundManager.playButtonClick();
				GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
				GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
				gem.playOpeningSequence();
			}
			else
			{
				SoundManager.soundManager.playButtonClick();
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
		LeanTween.move( episodeNamePanel, new Vector2(0, episodeNamePanel.rect.height/2f ), 0.5f ).setEase(LeanTweenType.easeOutQuad).setDelay(2.4f);
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
	
	public void displayStarPickup( int quantity, Color starColor )
	{
		if( PlayerStatsManager.Instance.getOwnsStarDoubler() ) quantity = quantity * 2;

		GameObject go = (GameObject)Instantiate(starPrefab);
		go.transform.SetParent( hudCanvas.transform, false );
		go.GetComponent<Image>().color = starColor;
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
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
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
			destroyAllPickupsDisplayed();
		}
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

}

