using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
	
	//Tap to play label displayed when ready to run
	GUIContent tapToPlayContent = new GUIContent( LocalizationManager.Instance.getText("MENU_TAP_TO_PLAY") );

	//For the 3,2,1 countdown displayed when resuming game after pausing it
	//Countdown 3,2,1
	int countdown = -1;
	//Sound to play every second during countdown
	AudioClip beep;
	Rect countdownRect;
	public GUIStyle countdownStyle;
	
	public GUIStyle saveMeLevelInfoStyle;

	
	//For the stats screen/Run Again display
	//For the  background
	Texture runAgainBackground;
	Rect runAgainBackgroundRect;
	//For the various stats that have both left and right-aligned text
	public GUIStyle statsScreenStyleLeft;
	public GUIStyle statsScreenStyleRight;
	//For the Run Again button
	GUIContent runAgainButtonContent;
	public GUIStyle runAgainStyle;
	GUIContent homeButtonContent;

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
	bool isDistanceNumberSpinning = false;
	
	//For displaying a message for each 1,000 meters run
	float distanceMarkerHeight = 0;
	LTRect distanceMarker;
	bool isShowingDistanceMarker = false;
	public GUIStyle distanceMarkerStyle;
	GUIContent witchTextContent = new GUIContent(System.String.Empty);
	Texture2D sliderImage;
	public Texture2D cuteWitch;
	int distanceMarkerInterval = 250; //in meters
	
	float timeScaleBeforePause;

	HUDSaveMe hudSaveMe;

	PopupHandler popupHandler;
	PlayerController playerController;

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
		beep = Resources.Load("Audio/beep") as AudioClip;
		
		//For countdown
		countdownRect = new Rect ( Screen.width * 0.5f, Screen.height * 0.5f, 24, 24 );

		//For the stats screen/Run Again display
		runAgainBackground = Resources.Load("GUI/runAgain") as Texture2D;
		runAgainBackgroundRect = new Rect ( 0, 0, Screen.width, Screen.height );
		
		runAgainButtonContent = new GUIContent ( LocalizationManager.Instance.getText("MENU_TRY_AGAIN") );
		homeButtonContent = new GUIContent ( LocalizationManager.Instance.getText("MENU_HOME") );

		//For displaying a message for each 1,000 meters run
		distanceMarkerHeight = Screen.height * 0.08f;
		distanceMarker = new LTRect( 0f, -distanceMarkerHeight, Screen.width, distanceMarkerHeight );

		hudSaveMe = GetComponent<HUDSaveMe>();

		//Adjust font sizes based on screen resolution
		PopupHandler.changeFontSizeBasedOnResolution( statsStyle );
		PopupHandler.changeFontSizeBasedOnResolution( coinAccumulatorStyle );
		PopupHandler.changeFontSizeBasedOnResolution( countdownStyle );
		PopupHandler.changeFontSizeBasedOnResolution( saveMeLevelInfoStyle );
		PopupHandler.changeFontSizeBasedOnResolution( statsScreenStyleLeft );
		PopupHandler.changeFontSizeBasedOnResolution( statsScreenStyleRight );
		PopupHandler.changeFontSizeBasedOnResolution( runAgainStyle );
		PopupHandler.changeFontSizeBasedOnResolution( distanceMarkerStyle );

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

		//For mobile - detect player taps
		detectTaps();

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
			if( gameState == GameState.Normal || gameState == GameState.Checkpoint )
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
				int coinTotal = PlayerStatsManager.Instance.getPlayerCoins();
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
			GUI.Label ( fpsRect, fps + "-" + LevelManager.Instance.getNextLevelToComplete() + "-" + playerController.getCurrentTileName() + "-"+playerController.currentLane,fpsStyle );

			if(GUI.Button( pauseRect, pauseButtonContent, pauseStyle ))
			{
				pauseGame();
			}
		}
		
		//Countdown related
		if( gameState == GameState.Countdown )
		{
			GUI.Label ( countdownRect, countdown.ToString(), countdownStyle );
		}
		
		//Tap to play label which is displayed when you are ready to run
		if( gameState == GameState.Menu )
		{
			displayRotatedLabel(tapToPlayContent, -4f, 0.4f );
		}

		//Save Me options
		if( gameState == GameState.SaveMe )
		{
			hudSaveMe.activatePopup(PopupType.SaveMe);
		}
		
		//Stats screen
		if( gameState == GameState.StatsScreen )
		{
			displayStatsScreen();
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
			pauseGame();
		}
		if (Input.GetMouseButtonDown(0))
		{
			startPlaying();
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

	//Returns the player to the Home menu (i.e. the World Map)
	public void goToHomeMenu()
	{
		Debug.Log("Home button pressed");
		//Save before going to home menu in particular so player does not lose stars he picked up
		PlayerStatsManager.Instance.savePlayerStats();
		playerController.enablePlayerControl(false);
		//We might have the slow down power-up still active, so just to be sure
		//we will reset the timescale back to 1.
		Time.timeScale = 1f;
		SoundManager.stopMusic();
		SoundManager.stopAmbience();
		showUserMessage = false;
		playerController.resetLevel();
		//Go back to world map
		Application.LoadLevel( 3 );
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus && GameManager.Instance.getGameState() != GameState.Paused ) pauseGame();
	}

	public void pauseGame()
	{
		GameState gameState = GameManager.Instance.getGameState();
		
		if( gameState == GameState.Normal )
		{
			//Pause game
			GameManager.Instance.setGameState( GameState.Paused );
			SoundManager.pauseMusic();
			timeScaleBeforePause = Time.timeScale;
			Time.timeScale = 0;
			AudioListener.pause = true;
			PauseMenu pauseMenu = GetComponent<PauseMenu>();
			popupHandler.setPauseMenu( pauseMenu );
			pauseMenu.setLevelToLoad( LevelManager.Instance.getNextLevelToComplete() );
			playerController.enablePlayerControl(false);
			popupHandler.activatePopup(PopupType.PauseMenu);
		}
		else if( gameState == GameState.Paused )
		{
			//We were paused. Start the resume game countdown
			GameManager.Instance.setGameState( GameState.Countdown );
			//We need to set the time scale back to 1 or else our coroutine will not execute.
			Time.timeScale = timeScaleBeforePause;
			popupHandler.closePopup();
			StartCoroutine(StartCountdown( 3 ));
		}
	}

	public IEnumerator StartCountdown( int startValue )
	{
		countdown = startValue;
		while (countdown > 0)
		{
			SoundManager.playGUISound( beep );
			//Slow time may be happening. We still want the countdown to be one second between count
			yield return new WaitForSeconds(1.0f * Time.timeScale);
			countdown --;
			if( countdown == 0 )
			{
				//Resume game
				AudioListener.pause = false;
				GameManager.Instance.setGameState( GameState.Normal );
				SoundManager.playMusic();
				playerController.enablePlayerControl(true);
				//Display a Go! message
				activateUserMessage( LocalizationManager.Instance.getText("GO"), 0.5f, 0f, 1.25f );

			}
	    }
	}

	void displayStatsScreen()
	{		
		
		if( !isDistanceNumberSpinning )
		{
			//Spin the distance total, then the coins total
			StartCoroutine( spinDistanceNumber( PlayerStatsManager.Instance.getDistanceTravelled() ) );
			isDistanceNumberSpinning = true;
		}

		//Draw background
		GUI.DrawTexture( runAgainBackgroundRect, runAgainBackground, ScaleMode.StretchToFill );
		
		//Draw distance - label plus number
		//Label
		Rect statsScreenRectLabels = new Rect ( Screen.width * 0.05f, Screen.height * 0.2f, 50, 24 );
		GUI.Label ( statsScreenRectLabels, LocalizationManager.Instance.getText("DISTANCE"), statsScreenStyleLeft );
		//Number
		Rect statsScreenRectNumbers = new Rect ( Screen.width * 0.75f, Screen.height * 0.2f, 50, 24 );
		GUI.Label ( statsScreenRectNumbers, distance.ToString("N0"), statsScreenStyleRight );
		
		//Draw coins - label plus number plus icon
		//Label
		statsScreenRectLabels.y = Screen.height * 0.3f;
		GUI.Label ( statsScreenRectLabels, LocalizationManager.Instance.getText("STARS"), statsScreenStyleLeft );
		//Number
		statsScreenRectNumbers.y = Screen.height * 0.3f;
		GUI.Label ( statsScreenRectNumbers, coins.ToString("N0"), statsScreenStyleRight );
		//Icon
		Rect statsScreenRectIcons = new Rect ( statsScreenRectNumbers.xMax + 8, statsScreenRectNumbers.yMax - 14, 36, 36 );
		GUI.color = Color.yellow;
		GUI.DrawTexture( statsScreenRectIcons, coinIconTexture,ScaleMode.ScaleToFit );
		GUI.color = Color.white;

		//Draw a semi-transparent separator line
		//Save the current color so we can restore it after
		Color originalColorValue = GUI.color;
		Color separatorColor = originalColorValue;
		separatorColor.a = 0.4f;
		GUI.color = separatorColor;
		Rect separatorRect = new Rect ( Screen.width * 0.05f, Screen.height * 0.38f, Screen.width * 0.86f, 12 );
		GUI.DrawTexture( separatorRect, runAgainBackground, ScaleMode.StretchToFill );
		GUI.color = originalColorValue;
		
		//Draw score (sum of distance plus coins)
		//Label
		statsScreenRectLabels.y = Screen.height * 0.4f;
		GUI.Label ( statsScreenRectLabels, LocalizationManager.Instance.getText("SCORE"), statsScreenStyleLeft );
		//Number
		statsScreenRectNumbers.y = Screen.height * 0.4f;
		int score = distance + coins;
		GUI.Label ( statsScreenRectNumbers, score.ToString("N0"), statsScreenStyleRight );
		
		//Draw Try again button
		Rect runAgainRect = GUILayoutUtility.GetRect( runAgainButtonContent, runAgainStyle );
		runAgainRect.x = (Screen.width-runAgainRect.width)/2f;
		runAgainRect.y = Screen.height * 0.71f;
		if( GUI.Button( runAgainRect, runAgainButtonContent, runAgainStyle ) )
		{
			SoundManager.playButtonClick();
			Debug.Log("Try Again button pressed");
			isDistanceNumberSpinning = false;
			wasHighScoreMessageDisplayedThisRun = false;
			showUserMessage = false;
			playerController.resetLevel();
			//Simply restart same level
			StartCoroutine(loadLevel());
		}

		//Draw Home (World Map) button
		runAgainRect = GUILayoutUtility.GetRect( homeButtonContent, runAgainStyle );
		runAgainRect.x = (Screen.width-runAgainRect.width)/2f;
		runAgainRect.y = Screen.height * 0.78f;
		if( GUI.Button( runAgainRect, homeButtonContent, runAgainStyle ) )
		{
			SoundManager.playButtonClick();
			Debug.Log("Home button pressed");
			isDistanceNumberSpinning = false;
			wasHighScoreMessageDisplayedThisRun = false;
			showUserMessage = false;
			playerController.resetLevel();
			//Go back to world map
			Application.LoadLevel( 3 );
		}
	
	}

	IEnumerator loadLevel()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		//Load level scene
		Application.LoadLevel( 4 );
		
	}

	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 1 )
			{
				startPlaying();
			}
		}
	}

	void startPlaying()
	{
		//If we are in the Menu state simply start running, but if we are in the OpeningSequence state
		//initiate the opening sequence instead.
		if (GameManager.Instance.getGameState() == GameState.Menu  )
		{
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
	
	IEnumerator spinDistanceNumber( int maxDistanceValue )
	{
		float duration = 2.2f;
		float startTime = Time.time;
		float elapsedTime = 0;

		int startValue = 0;
		int endValue = maxDistanceValue;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			distance =  (int) Mathf.Lerp( startValue, endValue, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
	    }
		StartCoroutine( spinCoinNumber( PlayerStatsManager.Instance.getPlayerCoins() ) );
		
	}
	
	IEnumerator spinCoinNumber( int maxCoinValue )
	{
		float duration = 2.2f;
		float startTime = Time.time;
		float elapsedTime = 0;

		int startValue = 0;
		int endValue = maxCoinValue;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			coins =  (int) Mathf.Lerp( startValue, endValue, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
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
	
	//Displays a horizontally centered label with a drop-shadow that has a rotation.
	private void displayRotatedLabel(GUIContent textContent, float labelAngle, float percentageHeight )
	{
		Rect textRect = GUILayoutUtility.GetRect( textContent, saveMeLevelInfoStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		
		Rect positionRect = new Rect( textCenterX, Screen.height * percentageHeight, textRect.width, textRect.height );
		
		//Save the GUI.matrix so we can restore it once our rotation is done
		Matrix4x4 matrixBackup = GUI.matrix;
		Vector2 pos = new Vector2( positionRect.x, positionRect.y );
		GUIUtility.RotateAroundPivot(labelAngle, pos);
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
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			isShowingDistanceMarker = false;
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

