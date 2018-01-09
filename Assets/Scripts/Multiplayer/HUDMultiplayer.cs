using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Apple.ReplayKit;
using System;
using System.Text;

public enum DebugInfoType
{
	NONE = 0,
	FPS = 1,
	NETWORK = 2,
	LATENCY = 3,
	FRAME_RATE_TEST = 4,
	EMOTES_TEST = 5,
	DONT_SPAWN_ZOMBIES = 6,
	SHOW_RUN_SPEED = 7
}

public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	const float DELAY_BEFORE_COUNTDOWN_STARTS = 3f;
	const float DELAY_WHEN_NOT_SHOWING_EMOTES = 9f;
	const float DELAY_WHEN_SHOWING_EMOTES = 15f;
	const float DELAY_WHEN_TESTING_EMOTES = 60f;
	const float DELAY_WHEN_NOT_ALL_PLAYERS_ARRIVED = 6f;
	PlayerRace localPlayerRace;
	PlayerControl localPlayerControl;
	PlayerRun localPlayerRun;
	[SerializeField] GameObject canvasGroupForFading;
	[Header("Distance Remaining")]
	[SerializeField] GameObject distanceRemaining; 	//Distance remaining gets hidden in coop mode.
	[SerializeField] Image distanceRemainingCounterRed;  //Radial 360 image
	[SerializeField] TextMeshProUGUI distanceText;
	[Header("Countdown")]
	[SerializeField] AudioClip beep; //Sound to play every second during countdown
	[SerializeField] TextMeshProUGUI goMessageText;
	[Header("User Message")]
	[SerializeField] TextMeshProUGUI userMessageText;
	[Header("Race Position")]
	[SerializeField] GameObject racePosition;
	[SerializeField] TextMeshProUGUI racePositionText;
	[Header("Debug Info")]
	[SerializeField] TextMeshProUGUI debugInfo;
	DebugInfoType debugInfoType;
	FPSCalculator fpsCalculator;
	[Header("Race About To End Message or Spectating message")]
	[SerializeField] TextMeshProUGUI topMessageText;
	[Header("Minimap")]
	[SerializeField] PhotonView minimapPhotonView;
	[Header("Emotes")]
	[SerializeField] GameObject emotePanel;
	[Header("Health Bar")]
	[SerializeField] HealthBarHandler healthBarHandler;
	[SerializeField] Image damageTakenEffect;
	[Header("Stasis Tap Instructions")]
	[SerializeField] GameObject stasisTapInstructions;
	[Header("Results Screen - not coop")]
	[SerializeField] GameObject resultsScreen;
	[Header("Results Screen - coop")]
	[SerializeField] GameObject coopResultsScreen;

	#region Other variables
	GenerateLevel generateLevel;
	#endregion

	//Event management used to notify players to start running
	public delegate void StartRunningEvent();
	public static event StartRunningEvent startRunningEvent;

	void Awake()
	{
		hudMultiplayer = this;
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();

		displayRacePosition( false );

		//HUD Debug Info
		debugInfoType = GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
		debugInfo.gameObject.SetActive( debugInfoType != DebugInfoType.NONE );
		fpsCalculator = GetComponent<FPSCalculator>();
		fpsCalculator.enabled = (debugInfoType == DebugInfoType.FPS || debugInfoType == DebugInfoType.FRAME_RATE_TEST || debugInfoType == DebugInfoType.DONT_SPAWN_ZOMBIES );

		userMessageText.gameObject.SetActive( false );
		goMessageText.gameObject.SetActive( false );
		canvasGroupForFading.GetComponent<CanvasGroup>().alpha = 0;
		resultsScreen.GetComponent<CanvasGroup>().alpha = 0;
		coopResultsScreen.GetComponent<CanvasGroup>().alpha = 0;
		distanceRemaining.SetActive( !GameManager.Instance.isCoopPlayMode() );
	}

	public void registerLocalPlayer ( Transform localPlayer )
	{
		localPlayerRace = localPlayer.GetComponent<PlayerRace>();
		localPlayerControl = localPlayer.GetComponent<PlayerControl>();
		localPlayerRun = localPlayer.GetComponent<PlayerRun>();
	}

	public string getLocalPlayerName ()
	{
		return localPlayerRace.name;
	}

	public void displayTopMessage ( string message )
	{
		if( string.IsNullOrEmpty( message ) )
		{
			topMessageText.text = string.Empty;
			topMessageText.gameObject.SetActive( false );
		}
		else
		{
			topMessageText.text = message;
			topMessageText.gameObject.SetActive( true );
		}
	}

	public HealthBarHandler getHealthBarHandler()
	{
		return healthBarHandler;
	}

	public void startCountdown()
	{
		StartCoroutine("countdown");
	}

	/// <summary>
	/// Flashes a damage effect on the HUD.
	/// </summary>
	public IEnumerator displayDamageEffect()
	{
		float maxAlpha = 0.85f;
		damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, 0 );
		float duration = 0.5f;
		float elapsedTime = 0;
		
		float startAlpha = 0;
		float endAlpha = maxAlpha;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			float alpha = Mathf.Lerp( startAlpha, endAlpha, elapsedTime/duration );
			damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, alpha );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
		damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, maxAlpha );

		yield return new WaitForSeconds( 0.85f );

		duration = 0.4f;
		elapsedTime = 0;
		
		startAlpha = maxAlpha;
		endAlpha = 0;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			float alpha = Mathf.Lerp( startAlpha, endAlpha, elapsedTime/duration );
			damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, alpha );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
	}

	/// <summary>
	/// Displays a permanent damage effect on the HUD.
	/// </summary>
	public IEnumerator displayPermanentDamageEffect()
	{
		float duration = 0.5f;
		float elapsedTime = 0;	

		float startAlpha = 0;
		damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, startAlpha );

		float endAlpha = 0.85f;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			float alpha = Mathf.Lerp( startAlpha, endAlpha, elapsedTime/duration );
			damageTakenEffect.color = new Color( damageTakenEffect.color.r, damageTakenEffect.color.g, damageTakenEffect.color.b, alpha );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
	}

	IEnumerator countdown()
	{
		//Give a few seconds for the player to get used to the scene before starting the countdown
		yield return new WaitForSeconds( DELAY_BEFORE_COUNTDOWN_STARTS );

		int countdownNumber = 3;
		while( countdownNumber > 0 )
		{
			goMessageText.text = countdownNumber.ToString();
			goMessageText.gameObject.SetActive( true );
			UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSeconds( 1f);
			countdownNumber--;
			goMessageText.gameObject.SetActive( false );
		}

		//Tell the players to start running
		if(startRunningEvent != null) startRunningEvent();
		//Display a Go! message and hide after a few seconds
		goMessageText.text = LocalizationManager.Instance.getText("GO");
		goMessageText.gameObject.SetActive( true );
		Invoke ("hideGoText", 1.5f );
		//Race is starting
		raceHasStarted = true;
		displayRacePosition( true );	
		canvasGroupForFading.GetComponent<FadeInCanvasGroup>().fadeIn();
		
	}

	public void startEndOfRaceCountdown()
	{
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAlone )
		{
			//Since we are playing alone, there is no need for a end of race countdown.
			//Let's wait for a few seconds for the victory animation to finish and then leave the room.
			StartCoroutine("leaveRoomShortly");
		}
		else
		{
			StartCoroutine("endOfRaceCountdown");
		}
	}

	IEnumerator endOfRaceCountdown()
	{
		topMessageText.gameObject.SetActive( true );

		int countdownNumber = 10;
		while( countdownNumber > 0 )
		{
			topMessageText.text = "Race ends in " + countdownNumber.ToString() + " sec.";
			//UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSeconds( 1f);
			countdownNumber--;
		}	
		topMessageText.gameObject.SetActive( false );
		#if UNITY_IOS
		try
		{
			if( ReplayKit.isRecording ) ReplayKit.StopRecording();
		}
   		catch (Exception e)
		{
			Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
    	}
		yield return new WaitForEndOfFrame();
		#endif 
		StartCoroutine( endOfRaceSlowdown() );
	}

	IEnumerator endOfRaceSlowdown()
	{
		float duration = 2.5f;
		float elapsedTime = 0;
		float startTimeScale = 1f;
		//If the local player has crossed the finish line, don't delay any further and leave the room.
		if( localPlayerRace.playerCrossedFinishLine )
		{
			//If we are here, it means that the 10 second end of race countdown has completed.
			//The local player has crossed the finish line.
			//Display the results.
			//If opponents have not crossed the finish line at this time, their result will still be displayed, but
			//their race duration will be "N/A".
			StartCoroutine( displayResultsAndEmotesScreen( 0 ) );
			yield return new WaitForSeconds( DELAY_WHEN_NOT_ALL_PLAYERS_ARRIVED );
			GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
			PhotonNetwork.LeaveRoom();
			yield break;
		}
		//However, if the local player has not crossed the finish line by the time the 10 second countdown has finished,
		//remove player control, gradually slowdown the world, and then leave the room.
		else
		{
			//Remove player control
			localPlayerRace.GetComponent<PlayerControl>().enablePlayerControl( false );
			//Display a Defeat text
			string defeat = LocalizationManager.Instance.getText("RACE_DEFEAT");
			activateUserMessage( defeat, 0, 2f );
			//Slowdown the world
			do
			{
				elapsedTime = elapsedTime + Time.unscaledDeltaTime;
				Time.timeScale = Mathf.Lerp( startTimeScale, 0, elapsedTime/duration );
				//Also change Time.fixedDeltaTime or else the Cinemachine camera will become jerky
				Time.fixedDeltaTime = GameManager.DEFAULT_FIXED_DELTA_TIME * Time.timeScale;
				yield return new WaitForEndOfFrame();  
				
			} while ( elapsedTime < duration );
			//Leave room
			GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
			PhotonNetwork.LeaveRoom();
		}
	}

	public IEnumerator leaveRoomShortly()
	{
		topMessageText.gameObject.SetActive( false );
		StopCoroutine("endOfRaceCountdown");
		if( debugInfoType == DebugInfoType.EMOTES_TEST )
		{
			//Stay longer because we are testing emotes
			yield return new WaitForSeconds( DELAY_WHEN_TESTING_EMOTES );
		}
		else if( GameManager.Instance.isOnlinePlayMode() && PlayerRace.players.Count > 1 )
		{
			//Stay longer in case the players want to exchange emotes
			yield return new WaitForSeconds( DELAY_WHEN_SHOWING_EMOTES );
		}
		else
		{
			yield return new WaitForSeconds( DELAY_WHEN_NOT_SHOWING_EMOTES );
		}
		#if UNITY_IOS
		try
		{
			if( ReplayKit.isRecording ) ReplayKit.StopRecording();
		}
   		catch (Exception e)
		{
			Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
    	}
		yield return new WaitForEndOfFrame();
		#endif
		if( !GameManager.Instance.isCoopPlayMode() ) GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
		PhotonNetwork.LeaveRoom();
	}

	/// <summary>
	/// Shows the emote panel if these conditions are met:
	/// We are in one of these play modes: PlayTwoPlayers, PlayThreePlayers or PlayWithFriends.
	/// There are at least two players at the end of the race. Some players may have quit or lost connection. That's why we check.
	/// </summary>
	void showEmotePanel()
	{
		if( GameManager.Instance.isOnlinePlayMode() || debugInfoType == DebugInfoType.EMOTES_TEST )
		{
			if( PlayerRace.players.Count > 1 || debugInfoType == DebugInfoType.EMOTES_TEST )
			{
				emotePanel.SetActive ( true );
			}
		}
	}

	void Update()
	{
		if( debugInfoType != DebugInfoType.NONE ) debugInfo.text = getDebugInfo();
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS && !GameManager.Instance.isCoopPlayMode() )
		{
			float distance = generateLevel.levelLengthInMeters - ( localPlayerControl.tileDistanceTraveled + localPlayerRace.distanceTravelledOnThisTile );
			if( distance <= 0.5f ) distance = 0; //Added as a safeguard.
			distanceText.text = distance.ToString("N0") + " <color=#FF396D><size=38><sub>M</sub></size></color>";
			distanceRemainingCounterRed.fillAmount = distance/generateLevel.levelLengthInMeters;
		}
	}

	#region Stasis Tap Instructions
	public void showTapInstructions( string tapInstructions )
	{
		stasisTapInstructions.GetComponentInChildren<TextMeshProUGUI>().text = tapInstructions;
		stasisTapInstructions.GetComponent<FadeInCanvasGroup>().fadeIn();
	}

	public void hideTapInstructions()
	{
		stasisTapInstructions.GetComponent<FadeInCanvasGroup>().fadeOut();
	}
	#endregion

	string getDebugInfo()
	{
		StringBuilder infoToDisplay = new StringBuilder();
		switch( debugInfoType )
		{
			case DebugInfoType.DONT_SPAWN_ZOMBIES:
			case DebugInfoType.FRAME_RATE_TEST:
			case DebugInfoType.FPS:
				infoToDisplay.Append( " FPS: " );
			 	infoToDisplay.Append( fpsCalculator.getFPS() ); 
			break;

			case DebugInfoType.NETWORK:
				infoToDisplay.Append( " isMaster: " );
				infoToDisplay.Append( PhotonNetwork.isMasterClient );
				infoToDisplay.Append( getDebugInfoForAllPlayers() );
			break;

			case DebugInfoType.LATENCY:
				infoToDisplay.Append( " isMaster: " );
				infoToDisplay.Append( PhotonNetwork.isMasterClient );
				infoToDisplay.Append( " Latency: " );
				infoToDisplay.Append(  PhotonNetwork.networkingPeer.RoundTripTime );
			break;

			case DebugInfoType.SHOW_RUN_SPEED:
				infoToDisplay.Append( " Local player run speed: " );
				infoToDisplay.Append( localPlayerRun.getRunSpeed() );
			break;
		}
		return infoToDisplay.ToString();
	}

	string getDebugInfoForAllPlayers()
	{
		StringBuilder infoForAllPlayers = new StringBuilder();
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			infoForAllPlayers.Append( " ");
		 	infoForAllPlayers.Append( PlayerRace.players[i].name );
			infoForAllPlayers.Append( " isMine: " );
			infoForAllPlayers.Append( PlayerRace.players[i].GetComponent<PhotonView>().isMine );
			infoForAllPlayers.Append( " " );
			infoForAllPlayers.Append( PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() );
		}
		return infoForAllPlayers.ToString();
	}

	void hideGoText()
	{
		goMessageText.gameObject.SetActive( false );
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	/// <summary>
	/// Updates the race position displayed in the HUD.
	/// </summary>
	/// <param name="position">Zero-indexed position i.e., 0 is the first place, 1, is second place, etc..</param>
	public void updateRacePosition( int racePosition )
	{
		racePositionText.text = getRacePositionAsString( racePosition + 1 );
	}

	string getRacePositionAsString( int racePosition )
	{		
		string ordinalIndicator;
		switch (racePosition)
		{
			case 1:
				ordinalIndicator = "1<size=64><sup>st</sup></size>";
				break;
				
			case 2:
				ordinalIndicator = "2<size=64><sup>nd</sup></size>";
				break;

			case 3:
				ordinalIndicator = "3<size=64><sup>rd</sup></size>";
				break;

			default:
				ordinalIndicator = string.Empty;
				break;
			
		}
		return ordinalIndicator;
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		CoopWaveGenerator.coopNewWave += CoopNewWave;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		CoopWaveGenerator.coopNewWave -= CoopNewWave;
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Normal )
		{
			if( raceHasStarted) displayRacePosition( true );
			debugInfo.gameObject.SetActive( debugInfoType != DebugInfoType.NONE );
		}
		else
		{
			displayRacePosition( false );
			debugInfo.gameObject.SetActive( false );
			userMessageText.gameObject.SetActive( false );
		}
	}

	void CoopNewWave( int waveNumber )
	{
		string waveString = LocalizationManager.Instance.getText("COOP_WAVE"); //Wave {0}!
		waveString = string.Format( waveString, waveNumber );
		activateUserMessage( waveString, 0, 2.5f );
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

	public PhotonView getMinimapPhotonView()
	{
		return minimapPhotonView;
	}

	public IEnumerator displayResultsAndEmotesScreen( float displayDelay )
	{
		yield return new WaitForSeconds( displayDelay );
		resultsScreen.GetComponent<ResultsScreenHandler>().showResults();
		resultsScreen.gameObject.SetActive( true );
		showEmotePanel();
	}

	public GameObject getEmoteGameObjectForPlayerNamed( string playerName )
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			return coopResultsScreen.GetComponent<CoopResultsScreenHandler>().getEmoteGameObjectForPlayerNamed( playerName );
		}
		else
		{
			return resultsScreen.GetComponent<ResultsScreenHandler>().getEmoteGameObjectForPlayerNamed( playerName );
		}
	}

	public IEnumerator displayCoopResultsAndEmotesScreen( float displayDelay )
	{
		yield return new WaitForSeconds( displayDelay );
		coopResultsScreen.gameObject.SetActive( true );
		coopResultsScreen.GetComponent<CoopResultsScreenHandler>().showResults();
		showEmotePanel();
	}

}
