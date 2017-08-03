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
	float distanceTravelled = 0;
	const float REQUIRED_LEAD_DISTANCE = 5f;
	//Cache the string to avoid the runtime lookup
	string tookTheLeadString;
	
	//Race duration which will get displayed in the end of race screen
	float raceDuration = 0;

	//Control variables
	bool raceStarted = false;
	bool playerCrossedFinishLine = false;

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

	void Start()
	{
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null )
		{	
			//Reset value. PlayerRaceManager is an instance and does not get re-created when the level reloads	
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.NOT_STARTED );
			players.Clear();
			officialRacePositionList.Clear();
			LevelManager.Instance.distanceTravelled = 0;
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
		if( !playerCrossedFinishLine && this.photonView.isMine && GetComponent<PlayerAI>() == null )
		{
			PlayerRaceManager.Instance.playerAbandonedRace();
		}
	}

	void StartRunningEvent()
	{
		Debug.Log("PlayerRace: received StartRunningEvent " + gameObject.name );
		raceStarted = true;
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null )
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
				if( this.photonView.isMine && GetComponent<PlayerAI>() == null )
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
			}
		}
	}

	//Only calculated on master client
	void updateDistanceTravelled()
	{
		//Do not take height into consideration for distance travelled
		Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
		Vector3 previous = new Vector3(previousPlayerPosition.x, 0, previousPlayerPosition.z);
		distanceTravelled = distanceTravelled + Vector3.Distance( current, previous );
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null ) LevelManager.Instance.distanceTravelled = distanceTravelled;
		previousPlayerPosition = transform.position;
	}

	void OnTriggerEnter(Collider other)
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( other.CompareTag("Finish Line") )
			{
				//Player has reached the finish line
				playerCrossedFinishLine = true;
				if( !officialRacePositionList.Contains(this) ) officialRacePositionList.Add( this );
				int officialRacePosition = officialRacePositionList.FindIndex(playerRace => playerRace == this);
				Debug.Log ("Finish Line crossed by " + gameObject.name + " in race position " + officialRacePosition + " players " + players.Count);
				this.photonView.RPC("OnRaceCompleted", PhotonTargets.AllViaServer, other.transform.position.z, raceDuration, distanceTravelled, officialRacePosition );
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
			racePosition = newRacePosition;
			//The bot has a photon view. This photon view, just like the player's, has isMine set to true. But we don't want a bot to affect the HUD, hence we make sure we are not a bot.
			if( this.photonView.isMine && GetComponent<PlayerAI>() == null ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
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
					GetComponent<PlayerIK>().isOvertaking( racePosition );
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
				if( GetComponent<PlayerAI>() == null )
				{
					//We're the player
					heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name.ToString();
				}
				else
				{
					//We're a bot
					heroName = 	GetComponent<PlayerAI>().botHero.userName;
				}
				MiniMap.Instance.displayMessage( string.Format( tookTheLeadString, heroName ) );
				GetComponent<PlayerVoiceOvers>().playVoiceOver(VoiceOverType.VO_Took_Lead);
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
	void OnRaceCompleted( float triggerPositionZ, float raceDuration, float distanceTravelled, int officialRacePosition )
    {
		Debug.Log("PlayerRace: OnRaceCompleted RPC received for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient +
			" isMine: " + this.photonView.isMine +
			" raceDuration: " + raceDuration + " distanceTravelled: " + distanceTravelled + " officialRacePosition: " + officialRacePosition );

		//Cancel all spell effects
		GetComponent<PlayerSpell>().cancelAllSpells();

		racePosition = officialRacePosition;

		//We want to slow down any player that reaches the finish line
		StartCoroutine( GetComponent<PlayerRun>().slowDownPlayerAfterFinishLine( 5f - (officialRacePosition * 1.4f), triggerPositionZ ) );

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
			GetComponent<PlayerControl>().setCharacterState(PlayerCharacterState.Idle);
			//Send a crossedFinishLine event to tell the HUD to remove the card timers
			if( crossedFinishLine != null ) crossedFinishLine( transform, officialRacePosition, GetComponent<PlayerAI>() != null );
			if( GetComponent<PlayerAI>() == null )
			{
				CancelInvoke("tookTheLead");
				string victory = LocalizationManager.Instance.getText("RACE_VICTORY");
				if( racePosition == 0 ) HUDMultiplayer.hudMultiplayer.activateUserMessage( victory, 0, 2.25f );
				HUDMultiplayer.hudMultiplayer.updateRacePosition(officialRacePosition + 1);
				GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<MultiplayerPauseMenu>().hidePauseButton();
				PlayerRaceManager.Instance.playerCompletedRace( (officialRacePosition + 1), raceDuration, distanceTravelled, GetComponent<PlayerControl>().getNumberOfTimesDiedDuringRace() );
			}
		}		
    }

	#region End of match voice over
	public void Victory_win_start ( AnimationEvent eve )
	{
		if( racePosition == 0 )
		{
			//Play a win Voice Over
			GetComponent<PlayerVoiceOvers>().playVoiceOver(VoiceOverType.VO_Win);
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
