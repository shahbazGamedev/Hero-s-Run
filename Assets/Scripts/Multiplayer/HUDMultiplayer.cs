using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Apple.ReplayKit;
using System;

public enum DebugInfoType
{
	NONE = 0,
	FPS = 1,
	NETWORK = 2,
	LATENCY = 3
}

public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	const float DELAY_BEFORE_COUNTDOWN_STARTS = 5f;
	const float DELAY_WHEN_NOT_SHOWING_EMOTES = 8f;
	const float DELAY_WHEN_SHOWING_EMOTES = 11f;
	[Header("Distance Traveled")]
	[SerializeField] GameObject distancePanel;
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
	[Header("Circuit Name and Icon Panel")]
	[SerializeField] RectTransform circuitDetailsPanel;
	[SerializeField] TextMeshProUGUI circuitNameText;
	[SerializeField] Image circuitIcon;
	[Header("Race About To End Message")]
	[SerializeField] Text raceEndingText;
	[Header("Minimap")]
	[SerializeField] PhotonView minimapPhotonView;
	[Header("Emotes")]
	[SerializeField] GameObject emotePanel;
	[Header("Health Bar")]
	[SerializeField] HealthBarHandler healthBarHandler;

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

		displayRacePosition( false );

		//HUD Debug Info
		debugInfoType = PlayerStatsManager.Instance.getDebugInfoType();
		debugInfo.gameObject.SetActive( debugInfoType != DebugInfoType.NONE );
		fpsCalculator = GetComponent<FPSCalculator>();
		fpsCalculator.enabled = (debugInfoType == DebugInfoType.FPS);

		userMessageText.gameObject.SetActive( false );
		distancePanel.SetActive( false );
	}
	
	void Start()
	{
		//Slide in circuit name and icon
		slideInCircuitDetails();
	}

	public HealthBarHandler getHealthBarHandler()
	{
		return healthBarHandler;
	}

	public void startCountdown()
	{
		StartCoroutine("countdown");
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
		distancePanel.SetActive( true );
		
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

		//Get a reference to the local player
		PlayerRace localPlayerRace = null;
		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null )
			{
			 	localPlayerRace = PlayerRace.players[i];
				break;
			}
		}

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
		showEmotePanel();
		if( GameManager.Instance.isOnlinePlayMode() && PlayerRace.players.Count > 1 )
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
		if( GameManager.Instance.isOnlinePlayMode() )
		{
			if( PlayerRace.players.Count > 1 )
			{
				emotePanel.SetActive ( true );
			}
		}
	}

	void Update()
	{
		if( debugInfoType != DebugInfoType.NONE ) debugInfo.text = getDebugInfo();
		distanceText.text = LevelManager.Instance.distanceTravelled.ToString("N0") + " m";
	}

	string getDebugInfo()
	{
		string infoToDisplay = string.Empty;
		switch( debugInfoType )
		{
			case DebugInfoType.FPS:
				infoToDisplay = " FPS: " + fpsCalculator.getFPS(); 
			break;

			case DebugInfoType.NETWORK:
				infoToDisplay = " isMaster: " + PhotonNetwork.isMasterClient + " " + getDebugInfoForAllPlayers(); 
			break;

			case DebugInfoType.LATENCY:
				infoToDisplay = " isMaster: " + PhotonNetwork.isMasterClient + " Latency: " + PhotonNetwork.networkingPeer.RoundTripTime.ToString(); 
			break;
		}
		return infoToDisplay;
	}

	string getDebugInfoForAllPlayers()
	{
		string infoForAllPlayers = string.Empty;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			infoForAllPlayers = infoForAllPlayers + "| " + PlayerRace.players[i].name + " isMine: " + PlayerRace.players[i].GetComponent<PhotonView>().isMine + " " + PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState();
		}
		return infoForAllPlayers;
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
				ordinalIndicator = "1<size=22>st</size>";
				break;
				
			case 2:
				ordinalIndicator = "2<size=22>nd</size>";
				break;

			case 3:
				ordinalIndicator = "3<size=22>rd</size>";
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

	void slideInCircuitDetails()
	{
		LevelData.MultiplayerInfo multiplayerInfo =	LevelManager.Instance.getSelectedCircuit();
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + multiplayerInfo.circuitInfo.sectorNumber.ToString() );
		circuitNameText.text = sectorName;
		circuitIcon.sprite = multiplayerInfo.circuitInfo.circuitIcon;
		LeanTween.move( circuitDetailsPanel, new Vector2(0, -circuitDetailsPanel.rect.height/2f), 0.5f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutCircuitDetails).setOnCompleteParam(gameObject);
	}

	void slideOutCircuitDetails()
	{
		//Wait a little before sliding back up
		LeanTween.move( circuitDetailsPanel, new Vector2(0, circuitDetailsPanel.rect.height/2f ), 0.5f ).setEase(LeanTweenType.easeOutQuad).setDelay(2.75f);
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
}
