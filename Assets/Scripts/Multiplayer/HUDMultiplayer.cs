using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	const float DELAY_BEFORE_COUNTDOWN_STARTS = 5f;
	[Header("Countdowm")]
	[SerializeField] AudioClip beep; //Sound to play every second during countdown
	[SerializeField] Text goText; //Same field as userMessageText in HUDHandler
	[Header("Race Position")]
	[SerializeField] GameObject racePosition;
	[SerializeField] Text racePositionText;
	[Header("Finish Flag")]
	[SerializeField] Image finishFlag;
	[Header("Latency")]
	[SerializeField] Text latency;
	[Header("Circuit Name and Icon Panel")]
	[SerializeField] RectTransform circuitDetailsPanel;
	[SerializeField] Text circuitNameText;
	[SerializeField] Image circuitIcon;

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
		latency.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		goText.gameObject.SetActive( false );
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

		goText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		goText.gameObject.SetActive( true );

		int countdownNumber = 3;
		while( countdownNumber > 0 )
		{
			goText.text = countdownNumber.ToString();
			UISoundManager.uiSoundManager.playAudioClip( beep );
			yield return new WaitForSecondsRealtime( 1f);
			countdownNumber--;
		}

		//Tell the players to start running
		if(startRunningEvent != null) startRunningEvent();
		//Display a Go! message and hide after a few seconds
		goText.rectTransform.eulerAngles = new Vector3( 0,0,4 );
		goText.text = LocalizationManager.Instance.getText("GO");
		Invoke ("hideGoText", 1.5f );
		//Race is starting
		raceHasStarted = true;
		displayRacePosition( true );	
		
	}

	void Update()
	{
		if( latency.gameObject.activeSelf ) latency.text = PhotonNetwork.networkingPeer.RoundTripTime.ToString();
	}

	void hideGoText()
	{
		goText.gameObject.SetActive( false );
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	public void displayFinishFlag( bool display )
	{
		finishFlag.gameObject.SetActive( display );
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
			latency.gameObject.SetActive( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() );
		}
		else
		{
			displayRacePosition( false );
			latency.gameObject.SetActive( false );
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

}
