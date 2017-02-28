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
	int racePosition = -1;
	int previousRacePosition = -2;	//Used to avoid updating if the value has not changed
	//Distance travelled. This is used to determine who is in 1st place, 2nd place, etc.
	Vector3 previousPlayerPosition = Vector3.zero;
	float distanceTravelled = 0;
	
	//Race duration which will get displayed in the end of race screen
	float raceDuration = 0;

	//Control variables
	bool raceStarted = false;
	bool playerCrossedFinishLine = false;

	//List of all PlayerRaces including the opponent(s)
	static public  List<PlayerRace> players = new List<PlayerRace> ();

	void Start()
	{
  		if (!players.Contains (this))
		{
            players.Add (this);
		}
	}

  	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
        if (players.Contains (this))
		{
            players.Remove (this);
		}
	}

	void StartRunningEvent()
	{
		Debug.Log("PlayerRace: received StartRunningEvent");
		raceStarted = true;
		if( this.photonView.isMine )
		{			
			PlayerRaceManager.Instance.raceStatus = RaceStatus.IN_PROGRESS;
			GameManager.Instance.playerStatistics.incrementNumberRacesRun();
		}
	}

	/// <summary>
	/// This method handles distance travelled, race position and race duration.
	///	We only want this to run on the master client as we don't want conflicting opinions about the race position for example. 
	/// </summary>
	void FixedUpdate()
	{
		if( PhotonNetwork.isMasterClient )
		{
			//Only calculate distance travelled, race position and race duration if the race is in progress. 
			if( raceStarted && !playerCrossedFinishLine )
			{
				//We use the distance travelled to determine the race position. All players on the master client need to do this.
				updateDistanceTravelled();

				//We only want the host to calculate the race position and race duration.
				//The host is the master client who is owned by this device (so IsMasterClient is true and IsMine is true).
				if( this.photonView.isMine )
				{
					//Update the race position of the players i.e. 1st place, 2nd place, and so forth
					//Order the list using the distance travelled
					players.Sort((x, y) => -x.distanceTravelled.CompareTo(y.distanceTravelled));

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
							players[i].previousRacePosition = players[i].racePosition;
							//Inform the player of his new position so that he can update it on the HUD
							players[i].photonView.RPC("OnRacePositionChanged", PhotonTargets.AllBufferedViaServer, newPosition );
						}
					}
					//Calculate the race duration. It will be sent at the end of the race.
					raceDuration = raceDuration + Time.deltaTime;
				}
			}
		}
	}

	//Only calculated on master client
	void updateDistanceTravelled()
	{
		//Important: The NetworkTransform component of the remote player can sometimes move a player backwards by a small amount.
		//So ONLY update the distance if the player moved forward or at least stayed in the same position.
		//The backward movement value is small but it will add up over time making the distance travelled  inacurate and this, in turn, will cause
		//the race position value to be wrong.
		if( transform.position.z >= previousPlayerPosition.z )
		{
			//Do not take height into consideration for distance travelled
			Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
			Vector3 previous = new Vector3(transform.position.x, 0, previousPlayerPosition.z);
			distanceTravelled = distanceTravelled + Vector3.Distance( current, previous );
			previousPlayerPosition = transform.position;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( other.CompareTag("Finish Line") )
			{
				//Player has reached the finish line
				playerCrossedFinishLine = true;
				Debug.Log ("Finish Line crossed by " + PhotonNetwork.player.NickName + " in race position " + racePosition );
				this.photonView.RPC("OnRaceCompleted", PhotonTargets.AllBuffered, other.transform.position.z, raceDuration, distanceTravelled );
			}
		}
	}

	[PunRPC]
	void readyToGo()
	{
		//Only MasterClient gets this RPC call
		GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>().playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void OnRacePositionChanged( int value )
	{
		racePosition = value;
		if( this.photonView.isMine ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
		//Debug.Log("PlayerRace: OnRacePositionChanged " +  (racePosition + 1 ) );
	}

	//This method is called when the player has crossed the finish line to let the client know the official race duration and distance travelled
	//as calculated by the MasterClient.
	[PunRPC]
	void OnRaceCompleted( float triggerPositionZ, float raceDuration, float distanceTravelled )
    {
		Debug.Log("PlayerRace: OnRaceCompleted RPC received for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient +
			" isMine: " + this.photonView.isMine + " isLocal: " + PhotonNetwork.player.IsLocal +
			" raceDuration: " + raceDuration + " distanceTravelled: " + distanceTravelled );

		//We want to slow down any player that reaches the finish line
		StartCoroutine( GetComponent<PlayerControl>().slowDownPlayerAfterFinishLine( 10f, triggerPositionZ ) );
		//However, in terms of changing HUD elements, XP, player stats, etc. We only want to proceed if the player is local.
		if( this.photonView.isMine )
		{
			HUDMultiplayer.hudMultiplayer.displayFinishFlag( true );
			GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<MultiplayerPauseMenu>().hidePauseButton();
			PlayerRaceManager.Instance.playerCompletedRace( (racePosition + 1), raceDuration, distanceTravelled, GetComponent<PlayerControl>().getNumberOfTimesDiedDuringRace() );
		}
    }

}
