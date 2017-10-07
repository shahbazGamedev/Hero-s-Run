// --------------------------------------------------------------------------------------------------------------------
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
	//Race position (1st place, 2nd place, etc.
	public int racePosition = -1;
	int previousRacePosition = -2;	//Used to avoid updating if the value has not changed
	//Distance travelled. This is used to determine who is in 1st place, 2nd place, etc.
	Vector3 previousPlayerPosition = Vector3.zero;
	public float distanceTravelled = 0;
	const float REQUIRED_LEAD_DISTANCE = 5f;
	//Cache the string to avoid the runtime lookup
	string tookTheLeadString;
	
	//Race duration which will get displayed in the end of race screen
	float raceDuration = 0;

	//Control variables
	bool raceStarted = false;
	public bool playerCrossedFinishLine = false;

	//List of all PlayerRaces including the opponent(s)
	static public List<PlayerRace> players = new List<PlayerRace> ();
	static public List<PlayerRace> officialRacePositionList = new List<PlayerRace> ();

	//Delegate used to communicate to other classes when the local player (and not a bot) has crossed the finish line.
	public delegate void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot );
	public static event CrossedFinishLine crossedFinishLine;

	//Number of tiles left to travel before reaching the end tile. Used to determine this player's race position.
	public int tilesLeftBeforeReachingEnd;
	public int numberPlayersBehindMe = 0;
	int previousNumberPlayersBehindMe = -1;

	#region Emergency Power Boost
	//The power boost is activated when a player is losing significantly to give him a chance to get back in the lead.
	//The power boost activates only once during a race.
	//The effect of the power boost is to increase the refill rate of the power bar and to increase the player's run speed.
	//Whether the power boost is active or not.
	bool isPowerBoostActive = false;
	//The number of tiles the player must be losing by for the power boost to activate.
	const int TILE_DIFFERENCE_ACTIVATOR = 3;
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
	PlayerIK playerIK;
	#endregion

	void Start()
	{
		//Cached for performance
		playerAI = GetComponent<PlayerAI>();
		playerRun = GetComponent<PlayerRun>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
		playerControl = GetComponent<PlayerControl>();
		playerSpell = GetComponent<PlayerSpell>();
		playerIK = GetComponent<PlayerIK>();


		if( this.photonView.isMine && playerAI == null )
		{	
			//Reset value. PlayerRaceManager is an instance and does not get re-created when the level reloads	
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.NOT_STARTED );
			players.Clear();
			officialRacePositionList.Clear();
			turnRibbonHandler = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
		}
 		if (!players.Contains (this))
		{
            players.Add (this);
		}
		//Cache the string to avoid the runtime lookup
		tookTheLeadString = LocalizationManager.Instance.getText( "MINIMAP_TOOK_LEAD" );
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
		raceStarted = true;
		if( this.photonView.isMine && playerAI == null )
		{			
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.IN_PROGRESS );
			GameManager.Instance.playerStatistics.incrementNumberRacesRun();
		}
	}

	/// <summary>
	/// This method handles distance travelled, race position and race duration.
	///	We only want this to run on the master client as we don't want conflicting opinions about the race position for example. 
	/// </summary>
	void FixedUpdate()
	{
		//Only calculate distance travelled, race position and race duration if the race is in progress. 
		if( raceStarted && !playerCrossedFinishLine )
		{
			//We use the distance travelled to determine the race position. All players on the master client need to do this.
			updateDistanceTravelled();
			if( PhotonNetwork.isMasterClient )
			{
				calculateNumberPlayersBehindMe();

				//We only want the host to calculate the race position and race duration. We don't want a bot to do it.
				//The host is the master client who is owned by this device (so IsMasterClient is true and IsMine is true).
				if( this.photonView.isMine && playerAI == null )
				{
					//Update the race position of the players i.e. 1st place, 2nd place, and so forth
					players = players.OrderBy( p => p.tilesLeftBeforeReachingEnd ).ThenByDescending( p => p.numberPlayersBehindMe ).ToList();

					for(int i=0; i<players.Count;i++)
					{
						//Find out where this player sits in the list
						int newPosition = players.FindIndex(a => a.gameObject == players[i].gameObject);

						//Verify if his position has changed
						if( newPosition != players[i].previousRacePosition )
						{
							//Yes, it has.
							//Save the new values
							players[i].racePosition = newPosition;
							players[i].previousRacePosition = newPosition;
							//Inform the player of his new position so that he can update it on the HUD
							if( !officialRacePositionList.Contains( this ) ) players[i].photonView.RPC("OnRacePositionChanged", PhotonTargets.AllViaServer, newPosition );
						}
					}
				}
				//Calculate the race duration. It will be sent at the end of the race.
				raceDuration = raceDuration + Time.deltaTime;
				verifyIfPowerBoostNeeded();
			}
		}
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

		//Calculate the tile difference between this player and the player immediately ahead.
		int indexOfPlayerImmediatelyAhead = racePosition - 1;
		int tileDifference = playersOrderedByRacePosition[racePosition].tilesLeftBeforeReachingEnd - playersOrderedByRacePosition[indexOfPlayerImmediatelyAhead].tilesLeftBeforeReachingEnd;

		//If this player is behind by more than TILE_DIFFERENCE_ACTIVATOR tiles the person ahead of him, activate the power boost.
		if( tileDifference > TILE_DIFFERENCE_ACTIVATOR )
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

	void updateDistanceTravelled()
	{
		//Do not take height into consideration for distance travelled
		Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
		Vector3 previous = new Vector3(previousPlayerPosition.x, 0, previousPlayerPosition.z);
		distanceTravelled = distanceTravelled + Vector3.Distance( current, previous );
		previousPlayerPosition = transform.position;
	}

	void OnTriggerEnter(Collider other)
	{
		if( this.photonView.isMine )
		{
			if( other.CompareTag("Finish Line") )
			{
				//Player has reached the finish line
				playerCrossedFinishLine = true;
				this.photonView.RPC("OnRaceCompleted", PhotonTargets.AllViaServer, raceDuration, racePosition );

				if( PhotonNetwork.isMasterClient )
				{
					if( !officialRacePositionList.Contains(this) ) officialRacePositionList.Add( this );
					int officialRacePosition = officialRacePositionList.FindIndex(playerRace => playerRace == this);
					Debug.Log ("Finish Line crossed by " + gameObject.name + " in race position " + officialRacePosition + " players " + players.Count);
					//if this is the first player to cross the finish line, start the End of Race countdown.
					if( officialRacePositionList.Count == 1 )
					{
						this.photonView.RPC("StartEndOfRaceCountdownRPC", PhotonTargets.AllViaServer );
					}
					else if( officialRacePositionList.Count == players.Count )
					{
						//Every player has crossed the finish line. We can stop the countdown and return everyone to the lobby.
						this.photonView.RPC("CancelEndOfRaceCountdownRPC", PhotonTargets.AllViaServer );
					}
				}
			}
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
	}

	[PunRPC]
	void readyToGo()
	{
		//Only MasterClient gets this RPC call
		GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>().playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void OnRacePositionChanged( int newRacePosition )
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
				print("OnRacePositionChanged new " + newRacePosition + " old " + racePosition + " " + gameObject.name );

			racePosition = newRacePosition;
			//The bot has a photon view. This photon view, just like the player's, has isMine set to true. But we don't want a bot to affect the HUD, hence we make sure we are not a bot.
			if( this.photonView.isMine && playerAI == null ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
			//Debug.Log("PlayerRace: OnRacePositionChanged " +  (racePosition + 1 )  + " name " + gameObject.name );
			if( players.Count >= 2)
			{
				if( racePosition == 0 )
				{
					CancelInvoke("tookTheLead");
					Invoke("tookTheLead", 1f );
				}
				else
				{
					CancelInvoke("tookTheLead");
					//We want the player to look at the other player overtaking him to add some emotion
					playerIK.isOvertaking( racePosition );
				}
			}
		}
	}

	void tookTheLead()
	{
		//Only proceed if we have at least two players. One or more players may have just disconnected.
		if( players.Count > 1 )
		{
			PlayerRace p2 = players.Find(a => a.racePosition == 1 );

			if( Vector3.Distance( transform.position, p2.transform.position) >= REQUIRED_LEAD_DISTANCE )
			{
				//Display a minimap message that this player or bot took the lead.
				string heroName;
				if( playerAI == null )
				{
					//We're the player
					heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name.ToString();
				}
				else
				{
					//We're a bot
					heroName = playerAI.botHero.userName;
				}
				print("tookTheLead " + heroName + " " + gameObject.name );
				MiniMap.Instance.displayMessage( string.Format( tookTheLeadString, heroName ) );
				playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Took_Lead);
			}
		}
	}

	public void playerDied()
	{
		//We don't want the player so say he is in the lead when he just died.
		CancelInvoke("tookTheLead");
	}

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

		//We want to slow down any player that reaches the finish line
		StartCoroutine( playerRun.slowDownPlayerAfterFinishLine( officialRacePosition, 5f - (officialRacePosition * 1.5f) ) );

		//Sanity check
		//tilesLeftBeforeReachingEnd should ALWAYS be zero when reaching the End tile. If it's not the case, make sure the tilePenalty
		//values are correct on shortcut tiles and that the teleporter has the right number of tiles skipped.
		if( tilesLeftBeforeReachingEnd != 0 ) Debug.LogError("The race is completed but tilesLeftBeforeReachingEnd is not equal to 0. Make sure the tilePenalty values are correct on shortcut tiles and that the teleporter has the right number of tiles skipped.");

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
				HUDMultiplayer.hudMultiplayer.updateRacePosition(officialRacePosition + 1);
				GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<MultiplayerPauseMenu>().hidePauseButton();
				GenerateLevel generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
				PlayerRaceManager.Instance.playerCompletedRace( (officialRacePosition + 1), raceDuration, generateLevel.levelLengthInMeters, playerControl.getNumberOfTimesDiedDuringRace() );
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

	void calculateNumberPlayersBehindMe()
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
	}
}
