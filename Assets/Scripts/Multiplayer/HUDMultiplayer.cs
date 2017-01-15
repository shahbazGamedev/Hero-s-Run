using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	public static HUDMultiplayer hudMultiplayer;
	bool raceHasStarted = false;
	[Header("Countdowm")]
	public AudioClip beep; //Sound to play every second during countdown
	public Text goText;
	[Header("Race Position")]
	public GameObject racePosition;
	public Text racePositionText;
	int previousPosition = -1;	//Used to avoid updating the canvas for nothing

	//Event management used to notify players to start running
	public delegate void StartRunningEvent();
	public static event StartRunningEvent startRunningEvent;

	//Ensure there is only one HUDMultiplayer
	void Awake()
	{
		//We don't want to display multiplayer information in Single player or call update for nothing
		if( !GameManager.Instance.isMultiplayer() ) Destroy( this );
		
		if (hudMultiplayer == null)
			hudMultiplayer = this;
		else if (hudMultiplayer != this)
			Destroy (gameObject);

		displayRacePosition( false );
	}
	
	public void initialiseCountdown()
	{
		goText.rectTransform.eulerAngles = new Vector3( 0,0,0 );
		goText.gameObject.SetActive( true );
	}

	public void updateCountdown( int countdown )
	{
		if( countdown > 0 )
		{
			goText.text = countdown.ToString();
			UISoundManager.uiSoundManager.playAudioClip( beep );
		}
		else
		{
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
	}

	void hideGoText()
	{
		goText.gameObject.SetActive( false );
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	public void updateRacePosition( int position )
	{
		//For performance reasons, avoid updating strings and canvas if the position has not changed since the last update
		if( position == previousPosition ) return;
		previousPosition = position;
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

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Normal )
		{
			if( raceHasStarted) displayRacePosition( true );
		}
		else
		{
			displayRacePosition( false );
		}
	}

}
