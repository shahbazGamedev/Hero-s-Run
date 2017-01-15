using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDMultiplayer : MonoBehaviour {

	[Header("General")]
	bool raceHasStarted = false;
	[Header("Countdowm")]
	public AudioClip beep; //Sound to play every second during countdown
	public Text goText;
	[Header("Race Position")]
	public GameObject racePosition;
	public Text racePositionText;
	int previousPosition = -1;	//Used to avoid updating the canvas for nothing
	bool first = false;

	//Event management used to notify players to start running
	public delegate void StartRunningEvent();
	public static event StartRunningEvent startRunningEvent;

	void Awake()
	{
		//We don't want to display multiplayer information in Single player or call update for nothing
		if( !GameManager.Instance.isMultiplayer() ) Destroy( this );
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
			InvokeRepeating( "toggleFirst", 4f, 4f );
		}
	}

	void hideGoText()
	{
		goText.gameObject.SetActive( false );
	}

	void toggleFirst()
	{
		first = !first;
	}

	void displayRacePosition( bool display )
	{
		racePosition.SetActive( display );
	}

	void Update()
	{
		//Update the race position of this player i.e. 1st place, 2nd place, and so forth
		updateRacePosition();
	}

	void updateRacePosition()
	{
		int position = getRacePosition();
		//For performance reasons, avoid updating strings and canvas if the position has not changed since the last update
		if( position == previousPosition ) return;
		previousPosition = position;
		racePositionText.text = getRacePositionAsString(position);
	}

	int getRacePosition()
	{
		if( first )
			return 1;
		else
			return 2;
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
