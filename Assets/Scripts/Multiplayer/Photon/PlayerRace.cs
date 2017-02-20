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
	int previousRacePosition = -1;	//Used to avoid updating if the value has not changed
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
		if( PhotonNetwork.player.IsLocal )
		{
			Debug.Log("PlayerRace: received StartRunningEvent");
			raceStarted = true;
			PlayerRaceManager.Instance.raceStatus = RaceStatus.IN_PROGRESS;
		}
	}

	void FixedUpdate()
	{
		if( PhotonNetwork.isMasterClient && !playerCrossedFinishLine )
		{
			//This is called on all the players on the MasterClient
			updateDistanceTravelled();
			//Update the race position of this player i.e. 1st place, 2nd place, and so forth
			//Order the list using the distance travelled
			players.Sort((x, y) => -x.distanceTravelled.CompareTo(y.distanceTravelled));
			//Find where we sit in the list
			int newPosition = players.FindIndex(a => a.gameObject == this.gameObject);
			if( newPosition != previousRacePosition )
			{
				racePosition = newPosition; 	//Race position is a syncvar
				this.photonView.RPC("OnRacePositionChanged", PhotonTargets.AllBufferedViaServer, racePosition );
				previousRacePosition = racePosition;
			}

			if( raceStarted ) raceDuration = raceDuration + Time.deltaTime;
		}
	}

	void updateDistanceTravelled()
	{
		if( PhotonNetwork.isMasterClient )
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
				this.photonView.RPC("OnRaceDurationChanged", PhotonTargets.AllBuffered, raceDuration );
				this.photonView.RPC("OnCrossingFinishLine", PhotonTargets.AllBufferedViaServer, other.transform.position.z );
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
		if( PhotonNetwork.player.IsLocal ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
		//Debug.Log("PlayerRace: OnRacePositionChanged " +  (racePosition + 1 ) );
	}

	[PunRPC]
	void OnCrossingFinishLine(float triggerPositionZ )
	{
		Debug.Log("PlayerRace: OnCrossingFinishLine RPC received for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient + " isMine: " + this.photonView.isMine + " isLocal: " + PhotonNetwork.player.IsLocal );		
		StartCoroutine( GetComponent<PlayerControl>().slowDownPlayerAfterFinishLine( 10f, triggerPositionZ ) );
		HUDMultiplayer.hudMultiplayer.displayFinishFlag( true );
		PlayerRaceManager.Instance.playerCrossedFinishLine( racePosition + 1 );
	}


	//This method is called when the player has crossed the finish line to let the client know the official race duration
	//as calculated by the MasterClient.
	[PunRPC]
	void OnRaceDurationChanged( float value )
    {
		raceDuration = value;
		PlayerRaceManager.Instance.raceDuration = raceDuration;
		Debug.Log("OnRaceDurationChanged: " + value );
    }

}
