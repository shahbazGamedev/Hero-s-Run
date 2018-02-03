﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Regis Geoffrion"></copyright>
// <summary>
//  Used in multiplayer
// </summary>
// <author>Régis Geoffrion</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public sealed class PlayerRace : Photon.PunBehaviour
{
	//List of all PlayerRaces including the opponent(s)
	static public List<PlayerRace> players = new List<PlayerRace> ();

	#region Race position
	public RacePosition racePosition = RacePosition.NOT_SET_1;
	public RacePosition previousRacePosition = RacePosition.NOT_SET_2;	//Used to avoid updating if the value has not changed
	#endregion

	#region Distance
	//Distance travelled on the current tile. This is used to calculate the distance remaining.
	public float distanceTravelledOnThisTile = 0;
	Vector3 previousPlayerPosition = Vector3.zero;
	public float distanceRemaining; //Used to determine the player's race position. The player with the smallest distance remaining is in the lead.
	#endregion

	#region Took the lead
	const float REQUIRED_LEAD_DISTANCE_SQUARED = 5f * 5f;
	const float REQUIRED_LEAD_TIME = 1f;
	//Cache the string to avoid the runtime lookup
	string tookTheLeadString;
	#endregion
	
	#region Race duration
	//Race duration which will get displayed in the end of race screen
	public float raceDuration = 0;
	#endregion

	#region Finish line
	public bool playerCrossedFinishLine = false;
	//Delegate used to communicate to other classes when the local player (and not a bot) has crossed the finish line.
	public delegate void CrossedFinishLine( Transform player, RacePosition officialRacePosition, bool isBot );
	public static event CrossedFinishLine crossedFinishLine;
	#endregion
	
	#region Emergency Power Boost
	//The power boost is activated when a player is losing significantly to give him a chance to get back in the lead.
	//The power boost activates only once during a race.
	//The effect of the power boost is to increase the refill rate of the power bar and to increase the player's run speed.
	//Whether the power boost is active or not.
	bool isPowerBoostActive = false; //only used by bots
	TurnRibbonHandler turnRibbonHandler;
	const float POWER_BOOST_DURATION = 19f;
	public bool wasPowerBoostUsed = false;
	#endregion

	#regionCached for performance
	PlayerAI playerAI;
	PlayerRun playerRun;
	PlayerVoiceOvers playerVoiceOvers;
	PlayerControl playerControl;
	PlayerSpell playerSpell;
	GenerateLevel generateLevel;
	LevelNetworkingManager levelNetworkingManager;
	string userName;
	#endregion

	void Awake()
	{
		//Cached for performance
		levelNetworkingManager = GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>();
	}

	void Start()
	{
		//Cached for performance
		playerAI = GetComponent<PlayerAI>();
		playerRun = GetComponent<PlayerRun>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
		playerControl = GetComponent<PlayerControl>();
		playerSpell = GetComponent<PlayerSpell>();
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
		distanceRemaining = generateLevel.levelLengthInMeters;
		storeUserName();
		//Cache the string to avoid the runtime lookup
		tookTheLeadString = LocalizationManager.Instance.getText( "MINIMAP_TOOK_LEAD" );

		if( this.photonView.isMine && playerAI == null )
		{	
			//Reset value. PlayerRaceManager is an instance and does not get re-created when the level reloads	
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.NOT_STARTED );
			players.Clear();
			turnRibbonHandler = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
		}
 		if (!players.Contains (this))
		{
            players.Add (this);
		}
	}

	void storeUserName()
	{
		if( playerAI == null )
		{
			//We're the player
			userName = GameManager.Instance.playerProfile.getUserName();
		}
		else
		{
			//We're a bot
			userName = playerAI.botHero.userName;
		}
	}

  	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

	/// <summary>
	/// This method gets reliably called when a player disconnects either by using the Pause menu and quitting or because of a loss of connection.
	/// </summary>
	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
		if (players.Contains (this))
		{
            players.Remove (this);
		}
		Debug.Log("PlayerRace: OnDisable() Players Count: " + players.Count + " " + gameObject.name );
		if( !playerCrossedFinishLine && this.photonView.isMine && playerAI == null )
		{
			PlayerRaceManager.Instance.playerAbandonedRace();
		}
	}

	void StartRunningEvent()
	{
		Debug.Log("PlayerRace: received StartRunningEvent " + gameObject.name );
		if( this.photonView.isMine && playerAI == null )
		{			
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.IN_PROGRESS );
			GameManager.Instance.playerStatistics.incrementNumberRacesRun();
		}
	}

	void Update()
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			calculateDistanceRemaining();
		}
	}

	void calculateDistanceRemaining()
	{
		//distanceRemaining is used to determine the race position.
		updateDistanceTravelled();
		distanceRemaining = generateLevel.levelLengthInMeters - ( playerControl.tileDistanceTraveled + distanceTravelledOnThisTile );
	}

	void updateDistanceTravelled()
	{
		//Do not take height into consideration for distance travelled
		Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
		Vector3 previous = new Vector3(previousPlayerPosition.x, 0, previousPlayerPosition.z);
		distanceTravelledOnThisTile = distanceTravelledOnThisTile + Vector3.Distance( current, previous );
		previousPlayerPosition = transform.position;
	}
	
	#region Power boost
	[PunRPC]
	void activatePowerBoostRPC()
	{
		isPowerBoostActive = true;
		if( photonView.isMine && playerAI == null )
		{
			HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("RACE_EMERGENCY_POWER_ENGAGED"), 0, 2.5f );
			turnRibbonHandler.increaseRefillRate();
		}
		playerRun.addSpeedMultiplier( SpeedMultiplierType.Power_Speed_Boost );

		Invoke( "disablePowerBoost", POWER_BOOST_DURATION );
		string emergencyPowerEngagedString = LocalizationManager.Instance.getText( "MINIMAP_EMERGENCY_POWER_ENGAGED" );
		MiniMap.Instance.displayMessage( string.Format( emergencyPowerEngagedString, userName ) );
	}

	void disablePowerBoost()
	{
		isPowerBoostActive = false;
		if( photonView.isMine && playerAI == null )
		{
			HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("RACE_EMERGENCY_POWER_DISENGAGED"), 0, 2.5f );
			turnRibbonHandler.resetRefillRate();
		}
		playerRun.removeSpeedMultiplier( SpeedMultiplierType.Power_Speed_Boost );
	}

	//This method is only used by bots.
	public bool isPowerBoostActivated()
	{
		return isPowerBoostActive;
	}
	#endregion

	void OnTriggerEnter(Collider other)
	{
		//Remember that Unity's physic engine can generate multiple OnTriggerEnter events for the same collision.
		//Only proceed if this player hasn't already crossed the finish line.
		if( other.CompareTag("Finish Line") && !playerCrossedFinishLine )
		{
			reachedFinishLine();
		}
	}

	void reachedFinishLine()
	{
		//Player has reached the finish line.
		//We cannot guarantee whether it is the local (i.e. isMine) or the remote player that will cross the finish line first.
		//We cannot guarantee whether it will be the Master or not-Master player that will cross the finish line first either.
		//Additionaly, with a latency of 300 msec. and a player speed of 20 m/sec, if we wait for an RPC sent by player A to player B before taking action,
		//player B will have traveled a whooping 6 meters.
		//Consequently, there are a some things we need to do immediately and some that can wait on an RPC to arrive.

		//Things to do immediately:
		//a) Set a local flag that the player crossed the finish line.
		//b) Cancel player spells.
		//c) Send a CrossedFinishLine event to interested classes.
		//d) Slow down the player.
		//e) Set the character state to Idle so this player cannot be affected by cards.
		//f) Cancel the took-the-lead invoke.
		//g) Cancel any power boost disengaged message.
		//h) Display a victory message.
		//i) Hide the Pause button.
		//j) Grant a "did-not-die-once" skill bonus if this is the case.
		//k) Activate the finish line camera.

		Debug.Log("PlayerRace-OnTriggerEnter: " +  name + " crossed the finish line in race position: " + racePosition );

		playerCrossedFinishLine = true;

		//Cancel all spell effects
		playerSpell.cancelAllSpells();

		//Send a crossedFinishLine event to interested classes.
		if( crossedFinishLine != null ) crossedFinishLine( transform, racePosition, playerAI != null );

		//We want to gradually slow down the player
		StartCoroutine( playerRun.slowDownPlayerAfterFinishLine( racePosition, 4.75f - ((int)racePosition * 2.25f) ) );

		//Note: if the player won, a voice over will be triggered by the victory animation. See Victory_win_start.

		//Set the character state to idle. When a character is idle, cards can't affect him. For example, we don't want a CardLightning spell to affect someone
		//who has crossed the finish line.
		playerControl.setCharacterState(PlayerCharacterState.Idle);

		//Don't activate took the lead if you have completed the race.
		CancelInvoke("tookTheLead");

		//Don't display a power boost disengaged message if you have completed the race.
		CancelInvoke( "disablePowerBoost" );

		if( this.photonView.isMine && playerAI == null )
		{
			//If the player won, display a victory message (but not in coop).
			if( racePosition == RacePosition.FIRST_PLACE && !GameManager.Instance.isCoopPlayMode() )
			{
				string victory = LocalizationManager.Instance.getText("RACE_VICTORY");
				HUDMultiplayer.hudMultiplayer.activateUserMessage( victory, 0, 2.25f );
			}
		
			//Hide the pause button
			GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<MultiplayerPauseMenu>().hidePauseButton();
					
			//if the player did not die a single time during the race and there is more than one player active, grant him a skill bonus.
			if( players.Count > 1 && playerControl.getNumberOfTimesDiedDuringRace() == 0 ) SkillBonusHandler.Instance.addSkillBonus( 50, "SKILL_BONUS_DID_NOT_DIE" );

			//Activate the finish line camera
			activateFinishLineCamera();
		}		
		
		//Things that can wait on the RPC to arrive:
		//a) Saving the official race data in player race manager.

		//This method informs the Level Networking Manager that a player has crossed the finish in line.
		//If this player is on Master, the Level Networking Manager will then send a OnRaceCompletedRPC to everyone.
		//This RPC will take care of saving the player race data.
		levelNetworkingManager.playerHasCrossedFinishLine( this );
	}

	public PhotonView getMinimapPhotonView()
	{
		return HUDMultiplayer.hudMultiplayer.getMinimapPhotonView();
	}

	[PunRPC]
	void StartEndOfRaceCountdownRPC()
	{
		HUDMultiplayer.hudMultiplayer.startEndOfRaceCountdown();
	}

	[PunRPC]
	void CancelEndOfRaceCountdownRPC()
	{
		PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
		StartCoroutine( HUDMultiplayer.hudMultiplayer.leaveRoomShortly() );
		//The Victory message remains on screen for 2.25 seconds.
		//So let's wait for 2.75 seconds before displaying the results and emotes panel.
		StartCoroutine( HUDMultiplayer.hudMultiplayer.displayResultsAndEmotesScreen( 2.75f ) );
	}

	[PunRPC]
	void readyToGo()
	{
		//Only MasterClient gets this RPC call
		levelNetworkingManager.playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void OnRacePositionChanged( RacePosition newRacePosition )
	{
		if( this.photonView.isMine && playerAI == null ) HUDMultiplayer.hudMultiplayer.updateRacePosition( newRacePosition );
		//Debug.Log("PlayerRace: OnRacePositionChanged " +  (newRacePosition + 1 )  + " name " + gameObject.name );

		racePosition = newRacePosition;

		verifyIfPlayerTookTheLead();
	}

	#region Took the lead
	/// <summary>
	/// Verifies if this player took the lead.
	/// Two things happen when the player takes the lead:
	/// a) A message is displayed on the minimap: "X took the lead."
	/// b) The player says a voice over.
	/// If after REQUIRED_LEAD_TIME, this player is still in first place and the distance between him and the player in second place is sufficient,
	/// then the message and VO will be activated.
	/// Nothing happens if there is only one player in the race.
	/// </summary>
	void verifyIfPlayerTookTheLead()
	{
		if( players.Count > 1 && !GameManager.Instance.isCoopPlayMode() )
		{
			if( racePosition == RacePosition.FIRST_PLACE )
			{
				//The player took the lead.
				Invoke("tookTheLead", REQUIRED_LEAD_TIME );
			}
			else
			{
				//The player is not in the lead.
				CancelInvoke("tookTheLead");
			}
		}
	}

	void tookTheLead()
	{
		//Only proceed if we have more than one player. One or more players may have disconnected.
		if( players.Count > 1 )
		{
			//Find the player in second place.
			PlayerRace p2 = players.Find(a => a.racePosition == RacePosition.SECOND_PLACE );

			//Is this player sufficiently ahead?
			if( Vector3.SqrMagnitude( transform.position - p2.transform.position ) > REQUIRED_LEAD_DISTANCE_SQUARED )
			{
				//Yes.
				//Display a minimap message that this player or bot took the lead and play a voice over.
				MiniMap.Instance.displayMessage( string.Format( tookTheLeadString, userName ) );
				playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Took_Lead);
			}
		}
	}

	public void playerDied()
	{
		//We don't want the player so say he is in the lead when he just died.
		CancelInvoke("tookTheLead");
	}
	#endregion

	#region Race completed
	[PunRPC]
	void OnRaceCompletedRPC( float raceDuration )
    {
		Debug.Log("PlayerRace: OnRaceCompleted RPC received for: " +  name + " raceDuration: " + raceDuration );
		this.raceDuration = raceDuration;

		if( this.photonView.isMine && playerAI == null )
		{
			//Save the race data in player race manager.
			PlayerRaceManager.Instance.playerCompletedRace( racePosition, raceDuration, playerControl.getNumberOfTimesDiedDuringRace() );
		}		
    }

	void activateFinishLineCamera()
	{
		Transform cameraLocation = playerControl.currentTile.transform.Find("End Camera");

		if( cameraLocation != null )
		{
			cameraLocation.GetComponent<Camera>().enabled = true;
		}
	}
	#endregion

	#region End of match voice over
	public void Victory_win_start ( AnimationEvent eve )
	{
		if( racePosition == RacePosition.FIRST_PLACE )
		{
			//Play a win Voice Over
			playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Win);
		}
	}
	#endregion
}
