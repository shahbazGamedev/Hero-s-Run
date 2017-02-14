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


[System.Serializable] public class ToggleEvent : UnityEvent<bool>{}

public class PlayerNetworkManager : Photon.PunBehaviour, IPunObservable
{
  	//Used to toggle components depending on whether they are shared, only for the local player, or only for the remote player.
  	[SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;
	
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

	//List of all PlayerNetworkManagers including the opponent(s)
	static public  List<PlayerNetworkManager> players = new List<PlayerNetworkManager> ();

	void Awake()
	{
	    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
	    DontDestroyOnLoad(gameObject);
	}
	
	void Start()
	{
        EnablePlayer ();

		if (!players.Contains (this))
		{
            players.Add (this);
		}
	
		//Tell the MasterClient that we are ready to go. Our level has been loaded and our player created.
		//The MasterClient will initiate the countdown
		if( this.photonView.isMine ) this.photonView.RPC("readyToGo", PhotonTargets.MasterClient, null );
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

    void EnablePlayer()
    {
        onToggleShared.Invoke (true);

		if (PhotonNetwork.player.IsLocal )
		{
            onToggleLocal.Invoke (true);
		}
        else
		{
            onToggleRemote.Invoke (true);
		}
    }

    void DisablePlayer()
    {
        onToggleShared.Invoke (false);

        if (PhotonNetwork.player.IsLocal)
		{
			onToggleLocal.Invoke (false);
		}
        else
		{
			onToggleRemote.Invoke (false);
		}
    }

	public void Die()
    {
		DisablePlayer ();
		Invoke ("Respawn", 5f );
    }

    void Respawn()
    {
        if (PhotonNetwork.player.IsLocal) 
        {
			//move player to respawn position
        }

        EnablePlayer ();
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
				this.photonView.RPC("OnRaceDurationChanged", PhotonTargets.AllViaServer, raceDuration );
				this.photonView.RPC("OnCrossingFinishLine", PhotonTargets.AllBufferedViaServer, other.transform.position.z );
				//if( isRaceFinished() ) returnToLobby();
				returnToLobby();
			}
		}
	}

    bool isRaceFinished()
    {
		bool raceFinished = true;
        for (int i = 0; i < players.Count; i++)
		{
			if( !players[i].playerCrossedFinishLine ) return false;
		}
		return raceFinished;
    }

	void returnToLobby()
	{
/*		if( PhotonNetwork.isMasterClient )
		{
	        for (int i = 0; i < players.Count; i++)
			{
		    	players[i].RpcGameOver (netId, name);
			}
			players.Clear();
		}
*/
		Invoke ("BackToLobby", 5f);

	}

    void BackToLobby()
    {
		PhotonNetwork.LeaveRoom();
    }

	void StartRunningEvent()
	{
		if( PhotonNetwork.player.IsLocal )
		{
			Debug.Log("PlayerNetworkManager: received StartRunningEvent");
			raceStarted = true;
			PlayerRaceManager.Instance.raceStatus = RaceStatus.IN_PROGRESS;
		}
	}
	[PunRPC]
	void readyToGo()
	{
		GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>().playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void OnRacePositionChanged( int value )
	{
		racePosition = value;
		if( PhotonNetwork.player.IsLocal ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
		Debug.Log("PlayerNetworkManager: OnRacePositionChanged " +  (racePosition + 1 ) );
	}

	[PunRPC]
	void OnCrossingFinishLine(float triggerPositionZ )
	{
		if( PhotonNetwork.player.IsLocal )
		{
			//Hack so players dont bump into each other
			if( racePosition == 0 ) transform.position = new Vector3( -1.3f, transform.position.y, transform.position.z );
			if( racePosition == 1 ) transform.position = new Vector3( 1.3f, transform.position.y, transform.position.z );
			GameManager.Instance.setGameState(GameState.MultiplayerEndOfGame);
			StartCoroutine( GetComponent<PlayerController>().slowDownPlayer( 5.5f, afterPlayerSlowdown, triggerPositionZ ) );
			HUDMultiplayer.hudMultiplayer.displayFinishFlag( true );
			PlayerRaceManager.Instance.playerCrossedFinishLine( racePosition + 1 );
		}
	}

	void afterPlayerSlowdown()
	{
		GetComponent<PlayerController>().playVictoryAnimation();
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

	//This method is used to satisfy the Interface.
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		gameObject.name = info.sender.NickName;
		Debug.Log("PlayerManager-OnPhotonInstantiate-Skin: " + info.sender.CustomProperties["Skin"] + " isMasterClient: " + PhotonNetwork.isMasterClient + " Name: " + info.sender.NickName );
		if ( PhotonNetwork.isMasterClient )
		{
			object[] stuff = new object[1];
			stuff[0] = info.sender.NickName;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (info.sender.CustomProperties["Skin"].ToString(), Vector3.zero, Quaternion.identity, 0, stuff);
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}			
	}
}
