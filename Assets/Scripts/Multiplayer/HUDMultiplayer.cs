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
	EMOTES_TEST = 5
}

public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	const float DELAY_BEFORE_COUNTDOWN_STARTS = 3f;
	const float DELAY_WHEN_NOT_SHOWING_EMOTES = 9f;
	const float DELAY_WHEN_SHOWING_EMOTES = 12f;
	const float DELAY_WHEN_TESTING_EMOTES = 120f;
	PlayerRace localPlayerRace;
	PlayerControl localPlayerControl;
	[SerializeField] GameObject canvasGroupForFading;
	[Header("Distance Remaining")]
	[SerializeField] Image distanceRemainingCounterRed;  //Radial 360 image
	[SerializeField] TextMeshProUGUI distanceText;
	[Header("Countdowm")]
	[SerializeField] AudioClip beep; //Sound to play every second during countdown
	[SerializeField] TextMeshProUGUI userMessageText;
	[Header("Race Position")]
	[SerializeField] GameObject racePosition;
	[SerializeField] TextMeshProUGUI racePositionText;
	[Header("Debug Info")]
	[SerializeField] TextMeshProUGUI debugInfo;
	DebugInfoType debugInfoType;
	FPSCalculator fpsCalculator;
	[Header("Race About To End Message")]
	[SerializeField] Text raceEndingText;
	[Header("Minimap")]
	[SerializeField] PhotonView minimapPhotonView;
	[Header("Emotes")]
	[SerializeField] GameObject emotePanel;
	[Header("Health Bar")]
	[SerializeField] HealthBarHandler healthBarHandler;
	[SerializeField] Image damageTakenEffect;
	[Header("Stasis Tap Instructions")]
	[SerializeField] GameObject stasisTapInstructions;
	[Header("Results Screen")]
	[SerializeField] GameObject resultsScreen;

	#region Other variables
	GenerateLevel generateLevel;
	#endregion

	//Event management used to notify players to start running
	public delegate void StartRunningEvent();
	public static event StartRunningEvent startRunningEvent;

	//Ensure there is only one HUDMultiplayer
	void Awake()
	{
		//We don't want to display multiplayer information in Single player or call update for nothing
		if( !GameManager.Instance.isMultiplayer() ) Destroy( gameObject );
		
		if (hudMultiplayer == null)
			hudMultiplayer = this;
		else if (hudMultiplayer != this)
			Destroy (gameObject);

		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();

		displayRacePosition( false );

		//HUD Debug Info
		debugInfoType = GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
		debugInfo.gameObject.SetActive( debugInfoType != DebugInfoType.NONE );
		fpsCalculator = GetComponent<FPSCalculator>();
		fpsCalculator.enabled = (debugInfoType == DebugInfoType.FPS || debugInfoType == DebugInfoType.FRAME_RATE_TEST );

		userMessageText.gameObject.SetActive( false );
		canvasGroupForFading.GetComponent<CanvasGroup>().alpha = 0;
		resultsScreen.GetComponent<CanvasGroup>().alpha = 0;
	}

	public void registerLocalPlayer ( Transform localPlayer )
	{
		localPlayerRace = localPlayer.GetComponent<PlayerRace>();
		localPlayerControl = localPlayer.GetComponent<PlayerControl>();
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

	IEnumerator countdown()
	{
		//Give a few seconds for the player to get used to the scene before starting the countdown
		yield return new WaitForSecondsRealtime( DELAY_BEFORE_COUNTDOWN_STARTS );

		userMessageText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		userMessageText.gameObject.SetActive( true );

		int countdownNumber = 3;
		while( countdownNumber > 0 )
		{
			userMessageText.text = countdownNumber.ToString();
			//UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1f);
			countdownNumber--;
		}

		//Tell the players to start running
		if(startRunningEvent != null) startRunningEvent();
		//Display a Go! message and hide after a few seconds
		userMessageText.rectTransform.eulerAngles = new Vector3( 0,0,4 );
		userMessageText.text = LocalizationManager.Instance.getText("GO");
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
		raceEndingText.gameObject.SetActive( true );

		int countdownNumber = 10;
		while( countdownNumber > 0 )
		{
			raceEndingText.text = "Race ends in " + countdownNumber.ToString() + " sec.";
			//UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1f);
			countdownNumber--;
		}	
		raceEndingText.gameObject.SetActive( false );
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
				yield return new WaitForEndOfFrame();  
				
			} while ( elapsedTime < duration );
			//Leave room
			GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
			PhotonNetwork.LeaveRoom();
		}
	}

	public IEnumerator leaveRoomShortly()
	{
		raceEndingText.gameObject.SetActive( false );
		StopCoroutine("endOfRaceCountdown");
		if( debugInfoType == DebugInfoType.EMOTES_TEST )
		{
			//Stay longer because we are testing emotes
			yield return new WaitForSecondsRealtime( DELAY_WHEN_TESTING_EMOTES );
		}
		else if( GameManager.Instance.isOnlinePlayMode() && PlayerRace.players.Count > 1 )
		{
			//Stay longer in case the players want to exchange emotes
			yield return new WaitForSecondsRealtime( DELAY_WHEN_SHOWING_EMOTES );
		}
		else
		{
			yield return new WaitForSecondsRealtime( DELAY_WHEN_NOT_SHOWING_EMOTES );
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
		GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
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

		float distance = generateLevel.levelLengthInMeters - ( localPlayerControl.tileDistanceTraveled + localPlayerRace.distanceTravelledOnThisTile );
		if( distance <= 0.5f ) distance = 0; //Added as a safeguard.
		distanceText.text = distance.ToString("N0") + " <color=#FF396D><size=38><sub>M</sub></size></color>";
		distanceRemainingCounterRed.fillAmount = distance/generateLevel.levelLengthInMeters;
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
		userMessageText.gameObject.SetActive( false );
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	public void updateRacePosition( int position )
	{
		racePositionText.text = getRacePositionAsString(position);
	}

	string getRacePositionAsString( int position )
	{		
		string ordinalIndicator;
		switch (position)
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
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
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

	public IEnumerator displayResultsAndEmotesScreen()
	{
		//The Victory message remains on screen for 2.25 seconds.
		//So let's wait for 2.75 seconds before displaying the results.
		yield return new WaitForSeconds( 2.75f );
		resultsScreen.GetComponent<ResultsScreenHandler>().showResults();
		resultsScreen.gameObject.SetActive( true );
		showEmotePanel();
	}

}
