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


[System.Serializable] public class ToggleEvent : UnityEvent<bool>{}

public class PlayerNetworkManager : Photon.PunBehaviour, IPunObservable
{
    [SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;
	[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
	public static GameObject LocalPlayerInstance;
	LevelNetworkingManager levelNetworkingManager;

	public int racePosition = -1;
	int previousRacePosition = -1;	//Used to avoid updating if the value has not changed
	public float raceDuration = 0;
	float internalRaceDuration = 0;

	[Header("Distance Travelled")]
	Vector3 previousPlayerPosition = Vector3.zero;
	public float distanceTravelled = 0;
	public bool raceStarted = false;
	public bool playerCrossedFinishLine = false;
	
	public void Awake()
	{
	    // #Important
	    // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
	    if (photonView.isMine)
	    {
	        LocalPlayerInstance = gameObject;
	    }
	
	    // #Critical
	    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
	    DontDestroyOnLoad(gameObject);
	}
	
	public void Start()
	{
		levelNetworkingManager = GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>();
        EnablePlayer ();
	
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
	}

    void EnablePlayer()
    {
        onToggleShared.Invoke (true);

        if (PhotonNetwork.player.IsLocal)
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

	[PunRPC]
	void readyToGo()
	{
	   levelNetworkingManager.playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}

	[PunRPC]
	void RpcCrossedFinishLine(float triggerPositionZ )
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

	void FixedUpdate()
	{
/*
		if( PhotonNetwork.isMasterClient && !playerCrossedFinishLine )
		{

			//This is called on all the players on the server
			updateDistanceTravelled();
			//Update the race position of this player i.e. 1st place, 2nd place, and so forth
			//Order the list using the distance travelled
			players.Sort((x, y) => -x.distanceTravelled.CompareTo(y.distanceTravelled));
			//Find where we sit in the list
			int newPosition = players.FindIndex(a => a.gameObject == this.gameObject);
			if( newPosition != previousRacePosition )
			{
				racePosition = newPosition; 	//Race position is a syncvar
				previousRacePosition = racePosition;
			}

			if( raceStarted ) internalRaceDuration = internalRaceDuration + Time.deltaTime;
		}
*/
	}

	void OnRacePositionChanged( int value )
	{
		racePosition = value;
		if( PhotonNetwork.player.IsLocal ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
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
				RpcCrossedFinishLine( other.transform.position.z );
				raceDuration = internalRaceDuration;
				//if( isRaceFinished() ) returnToLobby();
				returnToLobby();
			}
		}
	}

    public bool isRaceFinished()
    {
		/*
		bool raceFinished = true;
        for (int i = 0; i < players.Count; i++)
		{
			if( !players[i].playerCrossedFinishLine ) return false;
		}
		return raceFinished;
		*/
		return false;
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

	void OnRaceDurationChanged( float value )
    {
		raceDuration = value;
		PlayerRaceManager.Instance.raceDuration = raceDuration;
		Debug.Log("OnRaceDurationChanged: " + value );
    }

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
