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
using System.Linq;

/// <summary>
/// This class handles only race related events such as:
/// 1) Player position (e.g. 1st, 2nd, 3rd position). This is displayed in the HUD and used to decide the victor.
/// 2) Race Duration. This is displayed in the end of match screen.
/// 3) Whether the player crossed the finish line.
/// 4) Updating the HUD with race related events
/// </summary>
public class PlayerRace : Photon.PunBehaviour
{
	//List of all PlayerRaces including the opponent(s)
	static public List<PlayerRace> players = new List<PlayerRace> ();

	#region Race position
	public int racePosition = -1;
	public int previousRacePosition = -2;	//Used to avoid updating if the value has not changed
	#endregion

	#region Distance traveled on this tile.
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
	public delegate void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot );
	public static event CrossedFinishLine crossedFinishLine;
	#endregion
	
	#region Emergency Power Boost
	//The power boost is activated when a player is losing significantly to give him a chance to get back in the lead.
	//The power boost activates only once during a race.
	//The effect of the power boost is to increase the refill rate of the power bar and to increase the player's run speed.
	//Whether the power boost is active or not.
	bool isPowerBoostActive = false;
	//The distance the player must be losing by for the power boost to activate.
	const float PLAYER_DISTANCE_ACTIVATOR = 175f;
	TurnRibbonHandler turnRibbonHandler;
	const float POWER_BOOST_DURATION = 15f;
	bool wasPowerBoostUsed = false;
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

	#region Power Boost
	/// <summary>
	/// Verifies if a power boost is needed. Only called by the MasterClient.
	/// The power boost activates only once during a race.
	/// The effect of the power boost is to increase the refill rate of the power bar.
 	/// Power is what is used to play cards during a race. The power bar recharges at a constant rate up to a maximum.
	/// If a player is losing significantly, his recharge rate will be increased thus allowing him to play more cards, and hopefully get him back into the lead.
	/// In addition, his run speed increases for the duration.
	/// </summary>
	void verifyIfPowerBoostNeeded()
	{
		//The emergency power boost only triggers once during a race.
		if( wasPowerBoostUsed ) return;

		//The emergency power boost is irrelevant if you are playing alone.
		if( players.Count == 1 ) return;

		//The emergency power never triggers if you are in first place.
		if( racePosition == 0 ) return;

		//If we are here, it means that there are multiple players and this player is not in first place.
	
		//Get a list of players ordered by position.
		List<PlayerRace> playersOrderedByRacePosition = players.OrderBy( p => p.racePosition ).ToList();

		//Calculate the distance between this player and the player immediately ahead.
		int indexOfPlayerImmediatelyAhead = racePosition - 1;
		float distanceDifference = playersOrderedByRacePosition[racePosition].distanceRemaining - playersOrderedByRacePosition[indexOfPlayerImmediatelyAhead].distanceRemaining;
		//If this player is behind by more than PLAYER_DISTANCE_ACTIVATOR tiles the person ahead of him, activate the power boost.
		if( distanceDifference > PLAYER_DISTANCE_ACTIVATOR )
		{
			wasPowerBoostUsed = true;
			this.photonView.RPC("activatePowerBoostRPC", PhotonTargets.AllViaServer );
		}
	}

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
		print("Activating power boost for " + gameObject.name );
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
		print("Deactivating power boost for " + gameObject.name );
	}

	//This method is only used by bots.
	public bool isPowerBoostActivated()
	{
		return isPowerBoostActive;
	}
	#endregion

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Finish Line") )
		{
			//Player has reached the finish line
			playerCrossedFinishLine = true;
			
			levelNetworkingManager.playerHasCrossedFinishLine( this );

		}
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
		StartCoroutine( HUDMultiplayer.hudMultiplayer.leaveRoomShortly() );
		StartCoroutine( HUDMultiplayer.hudMultiplayer.displayResultsAndEmotesScreen() );
	}

	[PunRPC]
	void readyToGo()
	{
		//Only MasterClient gets this RPC call
		levelNetworkingManager.playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void OnRacePositionChanged( int newRacePosition )
	{
		if( this.photonView.isMine && playerAI == null ) HUDMultiplayer.hudMultiplayer.updateRacePosition( newRacePosition );
		Debug.Log("PlayerRace: OnRacePositionChanged " +  (newRacePosition + 1 )  + " name " + gameObject.name );

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
		if( players.Count > 1 )
		{
			if( racePosition == 0 )
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
			PlayerRace p2 = players.Find(a => a.racePosition == 1 );

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

	//This method is called when the player has crossed the finish line to let the client know the official race duration and distance travelled
	//as calculated by the MasterClient.
	[PunRPC]
	void OnRaceCompleted( float raceDuration, int officialRacePosition )
    {
		Debug.Log("PlayerRace: OnRaceCompleted RPC received for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient +
			" isMine: " + this.photonView.isMine +
			" raceDuration: " + raceDuration + " officialRacePosition: " + officialRacePosition );

		//Cancel all spell effects
		playerSpell.cancelAllSpells();

		racePosition = officialRacePosition;
		this.raceDuration = raceDuration;

		//We want to slow down any player that reaches the finish line
		StartCoroutine( playerRun.slowDownPlayerAfterFinishLine( officialRacePosition, 5f - (officialRacePosition * 1.5f) ) );

		//Note: if the player won, a voice over will be triggered by the victory animation. See Victory_win_start.

		//However, in terms of changing HUD elements, XP, player stats, etc. We only want to proceed if the player is local and not a bot.
		if( this.photonView.isMine )
		{
			//Set the character state to idle. When a character is idle, cards can't affect him. For example, we don't want a CardLightning spell to affect someone
			//who has crossed the finish line.
			playerControl.setCharacterState(PlayerCharacterState.Idle);
			//Send a crossedFinishLine event to tell the HUD to remove the card timers
			if( crossedFinishLine != null ) crossedFinishLine( transform, officialRacePosition, playerAI != null );
			if( playerAI == null )
			{
				CancelInvoke("tookTheLead");
				string victory = LocalizationManager.Instance.getText("RACE_VICTORY");
				if( racePosition == 0 ) HUDMultiplayer.hudMultiplayer.activateUserMessage( victory, 0, 2.25f );
				HUDMultiplayer.hudMultiplayer.updateRacePosition( officialRacePosition );
				GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<MultiplayerPauseMenu>().hidePauseButton();
				GenerateLevel generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
				PlayerRaceManager.Instance.playerCompletedRace( (officialRacePosition + 1), raceDuration, generateLevel.levelLengthInMeters, playerControl.getNumberOfTimesDiedDuringRace() );
				//if the player did not die a single time during the race and there is more than one player active, grant him a skill bonus.
				if( photonView.isMine && players.Count > 1 && playerControl.getNumberOfTimesDiedDuringRace() == 0 ) SkillBonusHandler.Instance.addSkillBonus( 50, "SKILL_BONUS_DID_NOT_DIE" );
				useFinishLineCamera();
			}
		}		
    }

	#region End of match voice over
	public void Victory_win_start ( AnimationEvent eve )
	{
		if( racePosition == 0 )
		{
			//Play a win Voice Over
			playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Win);
		}
	}
	#endregion

	void useFinishLineCamera()
	{
		Transform cameraLocation = playerControl.currentTile.transform.Find("End Camera");

		if( cameraLocation != null )
		{
			cameraLocation.GetComponent<Camera>().enabled = true;
		}
	}

	/*void calculateNumberPlayersBehindMe()
	{
		int count = 0;
		for(int i=0; i<players.Count;i++)
		{
			//Ignore yourself
			if( players[i] == this ) continue;
			count += getDotProduct( transform, players[i].transform.position );
			
		}
		if( count != previousNumberPlayersBehindMe )
		{
			numberPlayersBehindMe = count;
			previousNumberPlayersBehindMe = numberPlayersBehindMe;
		}
	}

	int getDotProduct( Transform player1, Vector3 player2Position )
	{
		Vector3 forward = player1.TransformDirection(Vector3.forward);
		Vector3 toOther = player2Position - player1.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}*/
}
