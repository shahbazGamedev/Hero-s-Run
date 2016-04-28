using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//The 3D world is displayed behind the HUD.
public class HUDHandler : MonoBehaviour {

	//Not used in game. Only used to count number of characters
	public string CharacterCounter = "";
	public int CharacterCount = 0;

	//Define each HUD elements in terms of content, layout and style
	
	//Distance and coins box
	Texture2D statsBoxTexture;
	Rect statsBoxRect;
	public GUIStyle statsStyle;
	
	//Distance travelled in meters
	Rect distanceRect;

	//Number of coins collected
	static Rect coinIconRect;
	Rect coinRect;
	
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
	Rect fpsRect;
	public GUIStyle fpsStyle;
	
	//Pause button
	Rect pauseRect;
	GUIContent pauseButtonContent;
	public GUIStyle pauseStyle;

	public GUIStyle saveMeLevelInfoStyle;

	//For the stats screen/Run Again display
	//For the  background
	//For the various stats that have both left and right-aligned text
	public GUIStyle statsScreenStyleLeft;
	public GUIStyle statsScreenStyleRight;

	//High score message
	bool wasHighScoreMessageDisplayedThisRun = false;

	//Generic user message
	static float userMessageStartTime = 0;
	public static bool showUserMessage = false;
	static string userText = "";
	static float userScreenHeightPercentage = 0;
	static float userAngle = 0;
	static float userMessageDuration = 0;
	
	//For spinning distance and coin total in the stats screen
	int distance = 0;
	int coins= 0;
	
	//For displaying a message for each 1,000 meters run
	float distanceMarkerHeight = 0;
	LTRect distanceMarker;
	bool isShowingDistanceMarker = false;
	public GUIStyle distanceMarkerStyle;
	GUIContent witchTextContent = new GUIContent(System.String.Empty);
	Texture2D sliderImage;
	public Texture2D cuteWitch;
	int distanceMarkerInterval = 250; //in meters

	public GameObject saveMeCanvas;
	HUDSaveMe hudSaveMe;

	PopupHandler popupHandler;
	PlayerController playerController;

	//New UI related
	//Tap to play button (the size of the canvas) with the Tap to play label
	//This is displayed when you start a level WHEN the game state changes to the MENU state.
	//Also see the waitForTapToPlay bool in LevelData
	public Button tapToPlayButton; 
	public Text tapToPlayText;

	//Pause Menu
	PauseMenu pauseMenu;

	void OnDrawGizmos ()
	{
		//Display the number of characters in the CharacterCounter field
		CharacterCount = CharacterCounter.Length; 
	}

	// Use this for initialization
	void Awake ()
	{
		Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		popupHandler = CoreManagers.GetComponent<PopupHandler>();
		playerController = GetComponent<PlayerController>();

		//initialize the stats box at the top
		statsBoxTexture = Resources.Load("GUI/emerland") as Texture2D;
		statsBoxRect = new Rect( 0, 0, Screen.width, 0.08f * Screen.height);
		float yPos = statsBoxRect.y + 10f;
		
		//initialize for distance
		distanceRect = new Rect ( Screen.width * 0.1f, yPos, 100, 40 );

		//initialize for coin total
		coinIconRect = new Rect ( Screen.width * 0.6f, yPos, Screen.width * 0.09f, Screen.width * 0.09f );
		coinRect = new Rect ( coinIconRect.xMax + 6  , yPos, 100, 40 );

		//Initialize for the fps counter
		fpsRect = new Rect ( Screen.width * 0.1f, Screen.height * 0.085f, 100, 24 );
		
		//Initilaize for pause
		float buttonSize = Screen.width * 0.11f;
		float buttonMargin = Screen.width * 0.03f;
		pauseRect = new Rect ( Screen.width  - buttonSize - buttonMargin, Screen.height - buttonSize - buttonMargin, buttonSize, buttonSize );
		//For the GUIStyle button states to work, GUIContent must either be an empty constructor or have text.
		//If you create a GUIContent with a texture, the button states will not change.
		pauseButtonContent = new GUIContent ();

		//For displaying a message for each 1,000 meters run
		distanceMarkerHeight = Screen.height * 0.08f;
		distanceMarker = new LTRect( 0f, -distanceMarkerHeight, Screen.width, distanceMarkerHeight );

		hudSaveMe = saveMeCanvas.GetComponent<HUDSaveMe>();

		//Adjust font sizes based on screen resolution
		PopupHandler.changeFontSizeBasedOnResolution( statsStyle );
		PopupHandler.changeFontSizeBasedOnResolution( coinAccumulatorStyle );
		PopupHandler.changeFontSizeBasedOnResolution( saveMeLevelInfoStyle );
		PopupHandler.changeFontSizeBasedOnResolution( statsScreenStyleLeft );
		PopupHandler.changeFontSizeBasedOnResolution( statsScreenStyleRight );
		PopupHandler.changeFontSizeBasedOnResolution( distanceMarkerStyle );

		//New UI related
		tapToPlayText.text = LocalizationManager.Instance.getText("MENU_TAP_TO_PLAY");
		GameObject pauseMenuManagerObject = GameObject.FindGameObjectWithTag("Pause Menu Manager");
		pauseMenu = pauseMenuManagerObject.GetComponent<PauseMenu>();

	}
	
	void Start()
	{
		//Display the name of the current level when the Run! button is displayed
		slideDistanceMarkerDown( LevelManager.Instance.getCurrentLevelName(), cuteWitch );
	}
		
	// Update is called once per frame
	void OnGUI ()
	{
		GameState gameState = GameManager.Instance.getGameState();

		int dist = PlayerStatsManager.Instance.getDistanceTravelled();
		if( dist != 0 && dist%distanceMarkerInterval == 0 && gameState != GameState.Checkpoint && playerController.getCharacterState() != CharacterState.Dying )
		{
			string complement = getTextComplement( dist );
			slideDistanceMarkerDown( complement, cuteWitch );
		}
		
		if( isShowingDistanceMarker )
		{
			showDistanceMarker();
		}
		else
		{
			//draw a semi-transparent stats box
			if( false && gameState == GameState.Normal || gameState == GameState.Checkpoint )
			{
				Color colPreviousGUIColor = GUI.color;
				GUI.color = new Color(colPreviousGUIColor.r, colPreviousGUIColor.g, colPreviousGUIColor.b, 0.3f);
				GUI.DrawTexture( statsBoxRect, statsBoxTexture );
				GUI.color = colPreviousGUIColor;
				
				//distance related
				int distance = PlayerStatsManager.Instance.getDistanceTravelled();
				GUI.Label ( distanceRect, distance.ToString("N0") + "M", statsStyle );
				
				//coin related
				//First, draw a coin icon
				GUI.color = Color.yellow;
				GUI.DrawTexture( coinIconRect, coinIconTexture,ScaleMode.ScaleToFit );
				GUI.color = Color.white;
				//Draw the coin total
				int coinTotal = PlayerStatsManager.Instance.getCurrentCoins();
				GUI.Label ( coinRect, coinTotal.ToString("N0"), statsStyle );
			}
		}

		//Pause button related and debugging
		if( gameState == GameState.Normal )
		{
			//fps related
			string isFirstTimePlaying = "-f";
			if( PlayerStatsManager.Instance.isFirstTimePlaying() ) isFirstTimePlaying = "-t";
			
			//GUI.Label ( fpsRect, fps + "-" + LevelManager.Instance.getCurrentLevelIndex() + "-" + PlayerStatsManager.Instance.getPlayerHighScore() + isFirstTimePlaying + "-" + PlayerController.getPlayerSpeed().ToString("N1") + "-" +  PlayerController.getPlayerSpeedBoost().ToString("N1"), fpsStyle );
			//GUI.Label ( fpsRect, fps + "-" + PlayerController.getPlayerSpeed().ToString("N1") + "-" + playerController.currentLane + "-" + playerController.desiredLane + "-tr-" + playerController.tileRotationY + "-" + playerController.getCharacterState() + "-" + playerController.lastSwipe + "-" + playerController.reasonDiedAtTurn + "-" + playerController.moveDirection.x.ToString("N1"),fpsStyle );
			GUI.Label ( fpsRect, fps + "-" + LevelManager.Instance.getNextLevelToComplete() + "-" + playerController.getCurrentTileName() + "-"+playerController.currentLane + "-" + PlayerController.getPlayerSpeed().ToString("N1"),fpsStyle );

			if(GUI.Button( pauseRect, pauseButtonContent, pauseStyle ))
			{
				pauseMenu.pauseGame();
			}
		}
				
		showCoinTotal();
		
		//User message if any
		if( showUserMessage && gameState == GameState.Normal )
		{
			//Display user message
			if( (Time.time - userMessageStartTime) < userMessageDuration )
			{	
				displayUserMessage();
			}
			else
			{
				showUserMessage = false;
			}
		}
	}
	
	// Update is called once per frame even when paused
	void Update ()
	{
		updateFPS();
		if( !wasHighScoreMessageDisplayedThisRun && PlayerStatsManager.Instance.isNewHighScore() )
		{
			showHighScoreMessage();
		}
		#if UNITY_EDITOR
		//For debugging
		if ( Input.GetKeyDown (KeyCode.W) ) 
		{
			pauseMenu.pauseGame();
		}
		#endif
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

	public void startPlaying()
	{
		//If we are in the Menu state simply start running, but if we are in the OpeningSequence state
		//initiate the opening sequence instead.
		if (GameManager.Instance.getGameState() == GameState.Menu  )
		{
			//Disable the Tap to play button (and the associated Tap to Play label) since we do not need it anymore.
			tapToPlayButton.gameObject.SetActive( false );
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

	private void showHighScoreMessage()
	{		
		wasHighScoreMessageDisplayedThisRun = true;
		activateUserMessage( LocalizationManager.Instance.getText("NEW_HIGH_SCORE"), 0.26f, 0, 3.5f );
	}
	
	//Activates a horizontally centered text with a drop-shadow.
	//User texts are only displayed in the Normal game state.
	public static void activateUserMessage( string text, float heightPercentage, float angle, float duration )
	{
		userText = text;
		userScreenHeightPercentage = heightPercentage;
		userAngle = angle;
		userMessageDuration = duration;
		userMessageStartTime = Time.time;
		showUserMessage = true;
	}
		
	//Displays a horizontally centered user text with a drop-shadow that was activated with the method: activateUserMessage.
	private void displayUserMessage()
	{
		GUIContent textContent = new GUIContent( userText );
		Rect textRect = GUILayoutUtility.GetRect( textContent, saveMeLevelInfoStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		
		Rect positionRect = new Rect( textCenterX, Screen.height * userScreenHeightPercentage, textRect.width, textRect.height );

		//Save the GUI.matrix so we can restore it once our rotation is done
		Matrix4x4 matrixBackup = GUI.matrix;
		Vector2 pos = new Vector2( positionRect.x, positionRect.y );
		GUIUtility.RotateAroundPivot(userAngle, pos);
		Utilities.drawLabelWithDropShadow( positionRect, textContent, saveMeLevelInfoStyle );
		GUI.matrix = matrixBackup;
	}

	public static Vector2 getCoinIconPos()
	{
		//Note: on the Mac, the perfect Y pos is: Screen.height - coinIconRect.yMax.
		//On the iPhone 5, however, the coins overshoot the icon slightly, hence the additional -coinIconRect.height/2f.
		Vector2 iconPos = new Vector2( coinIconRect.center.x, Screen.height - coinIconRect.yMax - coinIconRect.height/2f );
		return iconPos;
	}
	
	public void slideDistanceMarkerDown( string text, Texture2D image )
	{
		/*if( isShowingDistanceMarker ) 
		{
			Debug.Log("slideDistanceMarkerDown is busy displaying " + witchTextContent.text + " and cannot display new text " + text );
		}*/

		witchTextContent.text = text;
		if( image == null )
		{
			sliderImage = cuteWitch;
		}
		else
		{
			sliderImage = image;
		}

		isShowingDistanceMarker = true;

		LeanTween.move( distanceMarker, new Vector2(0, 0), 0.5f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideDistanceMarkerUp).setOnCompleteParam(gameObject).setDelay(0);
	}
	
	void showDistanceMarker()
	{
		GUI.BeginGroup( distanceMarker.rect );
        	GUI.Box( new Rect(0,0,Screen.width, distanceMarkerHeight), "" );
			Rect cuteWitchRect = new Rect(0,0,distanceMarkerHeight, distanceMarkerHeight);
			GUI.DrawTexture( cuteWitchRect, sliderImage );
			Rect positionRect = new Rect(distanceMarkerHeight,0,Screen.width-distanceMarkerHeight, distanceMarkerHeight);
			//Width and height of text area
			float textMargin = 0.95f; //to give a bit of breathing room to the text
			float availableWidth = (Screen.width-distanceMarkerHeight) * textMargin;
			float availableHeight = distanceMarkerHeight * textMargin;
			//Size required by text if using distanceMarkerStyle
			Vector2 textSize = distanceMarkerStyle.CalcSize( witchTextContent );
			//font ratio needed to ensure text is as big as it can be while fitting in the space
			float ratioW = availableWidth/textSize.x;
			float ratioH = availableHeight/textSize.y;
			float ratio = Mathf.Min(ratioW, ratioH);
			int originalFontSize = distanceMarkerStyle.fontSize;
			distanceMarkerStyle.fontSize = (int)(distanceMarkerStyle.fontSize * ratio);
			Utilities.drawLabelWithDropShadow ( positionRect, witchTextContent, distanceMarkerStyle );

			distanceMarkerStyle.fontSize = originalFontSize;
		GUI.EndGroup();
	}

	string getTextComplement( int dist )
	{
		string firstPart = dist.ToString("N0") + "M ";
		
		if( dist == distanceMarkerInterval )
		{
			return firstPart + LocalizationManager.Instance.getText("GOOD");
		}
		else if( dist == ( 2 * distanceMarkerInterval) )
		{
			return firstPart + LocalizationManager.Instance.getText("EXCELLENT");		
		}
		else if( dist == ( 3 * distanceMarkerInterval) )
		{
			return firstPart + LocalizationManager.Instance.getText("AWESOME");		
		}
		else
		{
			return firstPart + LocalizationManager.Instance.getText("STUPENDOUS");		
		}

	}
	
	void slideDistanceMarkerUp()
	{
		//Wait a little before sliding back up
		LeanTween.move( distanceMarker, new Vector2(0, -distanceMarkerHeight ), 0.5f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(hideDistanceMarker).setOnCompleteParam(gameObject).setDelay(2.4f);
	}
	
	void hideDistanceMarker()
	{
		isShowingDistanceMarker = false;
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
	
	private void showCoinTotal()
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
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;

	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;

	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			isShowingDistanceMarker = false;
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

