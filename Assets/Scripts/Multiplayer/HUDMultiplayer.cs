using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	const float DELAY_BEFORE_COUNTDOWN_STARTS = 5f;
	[Header("Distance Traveled")]
	[SerializeField] GameObject distancePanel;
	[SerializeField] Text distanceText;
	[Header("Countdowm")]
	[SerializeField] AudioClip beep; //Sound to play every second during countdown
	[SerializeField] Text userMessageText;
	[Header("Race Position")]
	[SerializeField] GameObject racePosition;
	[SerializeField] Text racePositionText;
	[Header("Finish Flag")]
	[SerializeField] Image finishFlag;
	[Header("Debug Info")]
	[SerializeField] Text debugInfo;
	FPSCalculator fpsCalculator;
	[Header("Circuit Name and Icon Panel")]
	[SerializeField] RectTransform circuitDetailsPanel;
	[SerializeField] Text circuitNameText;
	[SerializeField] Image circuitIcon;
	[Header("Race About To End Message")]
	[SerializeField] Text raceEndingText;
	[Header("Minimap")]
	[SerializeField] PhotonView minimapPhotonView;

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
		finishFlag.gameObject.SetActive( false );
		fpsCalculator = GetComponent<FPSCalculator>();
		fpsCalculator.enabled = PlayerStatsManager.Instance.getShowDebugInfoOnHUD();
		debugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		userMessageText.gameObject.SetActive( false );
		distancePanel.SetActive( false );
	}
	
	void Start()
	{
		//Slide in circuit name and icon
		slideInCircuitDetails();
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
			UISoundManager.uiSoundManager.playAudioClip( beep );
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
			UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1f);
			countdownNumber--;
		}
	
		GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
		PhotonNetwork.LeaveRoom();
	}

	public IEnumerator leaveRoomShortly()
	{
		raceEndingText.gameObject.SetActive( false );
		StopCoroutine("endOfRaceCountdown");
		yield return new WaitForSecondsRealtime( 5f );
		GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
		PhotonNetwork.LeaveRoom();
	}

	void Update()
	{
		if( debugInfo.gameObject.activeSelf ) debugInfo.text = " FPS: " + fpsCalculator.getFPS() + " Latency: " + PhotonNetwork.networkingPeer.RoundTripTime.ToString() + " Name: " + PhotonNetwork.playerName; 
		distanceText.text = LevelManager.Instance.distanceTravelled.ToString("N0") + " m";
	}

	void hideGoText()
	{
		userMessageText.gameObject.SetActive( false );
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	public void displayFinishFlag( bool display )
	{
		finishFlag.gameObject.SetActive( display );
		distancePanel.SetActive( false );
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
			debugInfo.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
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
		LevelData.MultiplayerInfo multiplayerInfo =	LevelManager.Instance.getSelectedMultiplayerLevel();
		circuitNameText.text = LocalizationManager.Instance.getText(multiplayerInfo.circuitInfo.circuitTextID );
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
