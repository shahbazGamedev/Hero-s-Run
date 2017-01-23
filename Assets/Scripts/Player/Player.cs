using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool>{}

public class Player : NetworkBehaviour 
{
    [SyncVar (hook = "OnNameChanged")] public string playerName;
    [SyncVar (hook = "OnColorChanged")] public Color playerColor;

    [SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;
    [SerializeField] float respawnTime = 5f;

    static public List<Player> players = new List<Player> ();

    NetworkAnimator anim;

	public GameObject Hero_Prefab;
	public GameObject Heroine_Prefab;
	public int skinIndexTest;
	public string myPlayerNameTest;

	[SyncVar (hook = "OnSkinIndexChanged" ) ] int skinIndex;
	[SyncVar (hook = "OnPlayerNameChanged" ) ] string myPlayerName;
	[SyncVar (hook = "OnRacePositionChanged" ) ] public int racePosition = -1;
	int previousRacePosition = -1;	//Used to avoid updating if the value has not changed
	[SyncVar (hook = "OnRaceDurationChanged" ) ] public float raceDuration = 0;
	float internalRaceDuration = 0;

	[Header("Distance Travelled")]
	Vector3 previousPlayerPosition = Vector3.zero;
	public float distanceTravelled = 0;
	public bool raceStarted = false;
	public bool playerCrossedFinishLine = false;

    [SyncVar (hook = "OnAnimationTriggerChanged")]
	int syncAnimationTrigger;

    void Start()
    {
        anim = GetComponent<NetworkAnimator> ();
  
        EnablePlayer ();
		if (!players.Contains (this))
		{
            players.Add (this);
		}
    }

    [ServerCallback]
    void OnDisable()
    {
        if (players.Contains (this))
		{
            players.Remove (this);
		}
 		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
   }

	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

    void DisablePlayer()
    {
        if (isLocalPlayer) 
        {
            //PlayerCanvas.canvas.HideReticule ();
         }

        onToggleShared.Invoke (false);

        if (isLocalPlayer)
            onToggleLocal.Invoke (false);
        else
            onToggleRemote.Invoke (false);
    }

    void EnablePlayer()
    {
        if (isLocalPlayer) 
        {
            //PlayerCanvas.canvas.Initialize ();
        }

        onToggleShared.Invoke (true);

        if (isLocalPlayer)
            onToggleLocal.Invoke (true);
        else
            onToggleRemote.Invoke (true);
    }

    public void Die()
    {
        if(isLocalPlayer || playerControllerId == -1)
            anim.SetTrigger ("Died");
        
        if (isLocalPlayer) 
        {
            //PlayerCanvas.canvas.WriteGameStatusText ("You Died!");
            //PlayerCanvas.canvas.PlayDeathAudio ();
        }

        DisablePlayer ();

        Invoke ("Respawn", respawnTime);
    }

    void Respawn()
    {
        if(isLocalPlayer || playerControllerId == -1)
            anim.SetTrigger ("Restart");

        if (isLocalPlayer) 
        {
            Transform spawn = NetworkManager.singleton.GetStartPosition ();
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }

        EnablePlayer ();
    }

    void OnNameChanged(string value)
    {
        playerName = value;
        gameObject.name = playerName;
        GetComponentInChildren<Text> (true).text = playerName;
    }

    void OnColorChanged(Color value)
    {
        playerColor = value;
        //GetComponentInChildren<RendererToggler> ().ChangeColor (playerColor);
    }

    [Server]
    public void Won()
    {
        for (int i = 0; i < players.Count; i++)
            players [i].RpcGameOver (netId, name);

        Invoke ("BackToLobby", 5f);
    }

	[ClientRpc]
	void RpcCrossedFinishLine(float triggerPositionZ )
	{
		if( isLocalPlayer )
		{
			//Hack so players dont bump into each other
			if( racePosition == 0 ) transform.position = new Vector3( -1.3f, transform.position.y, transform.position.z );
			if( racePosition == 1 ) transform.position = new Vector3( 1.3f, transform.position.y, transform.position.z );
			GameManager.Instance.setGameState(GameState.Checkpoint);
			StartCoroutine( GetComponent<PlayerController>().slowDownPlayer( 5.5f, afterPlayerSlowdown, triggerPositionZ ) );
			HUDMultiplayer.hudMultiplayer.displayFinishFlag( true );
		}
	}

	void afterPlayerSlowdown()
	{
		GetComponent<PlayerController>().playVictoryAnimation();
	}

    [ClientRpc]
    void RpcGameOver(NetworkInstanceId networkID, string name)
    {
        if (netId == networkID)
		{
			Debug.LogWarning("Player-RpcGameOver: " + networkID + " " + name + " race position: " + (racePosition + 1) );
		}
        /*DisablePlayer ();

        if (isLocalPlayer)
        {
            if (netId == networkID)
			{
                //PlayerCanvas.canvas.WriteGameStatusText ("You Won!");
			}
            else
			{
                //PlayerCanvas.canvas.WriteGameStatusText ("Game Over!\n" + name + " Won");
			}
        }*/
    }

	public void setSkin( int value )
    {
		skinIndex = value;
    }

	void OnSkinIndexChanged( int value )
    {
		skinIndex = value;
		skinIndexTest = skinIndex;
		loadPlayerSkin();
    }

	public void setPlayerName( string value )
    {
		myPlayerName = value;
    }

	void OnPlayerNameChanged( string value )
    {
		myPlayerName = value;
		myPlayerNameTest = myPlayerName;
    }
	
	void loadPlayerSkin()
	{
		if( GameManager.Instance.isMultiplayer() )
		{
			GameObject hero = null;
			if(skinIndex == (int)Avatar.Hero )
			{
				
				Debug.Log("loadPlayerSkin-Loading MALE prefab." );
				hero = (GameObject)Instantiate(Hero_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			else if(skinIndex == (int)Avatar.Heroine )
			{
				Debug.Log("loadPlayerSkin-Loading FEMALE prefab." );
				hero = (GameObject)Instantiate(Heroine_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			else
			{
				Debug.LogError("loadPlayerSkin-Unknown skin index: " + skinIndex );
			}
			if( hero != null )
			{
				hero.transform.parent = transform;
				hero.transform.localPosition = Vector3.zero;
				hero.transform.localRotation = Quaternion.identity;
		
				hero.name = "Hero";
				GetComponent<Animator>().avatar = hero.GetComponent<PlayerSkinInfo>().animatorAvatar;
				hero.SetActive( true );
			}
		}
	}

	[ServerCallback]
	void FixedUpdate()
	{
		if( !playerCrossedFinishLine )
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
	}

	void OnRacePositionChanged( int value )
	{
		racePosition = value;
		if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.updateRacePosition(racePosition + 1); //1 is first place, 2 is second place, etc.
	}

	[Server]
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

	[ServerCallback]
	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Finish Line") )
		{
			//Player has reached the finish line
			playerCrossedFinishLine = true;
			Debug.Log ("Finish Line crossed by " + netId + " in race position " + racePosition );
			RpcCrossedFinishLine( other.transform.position.z );
			raceDuration = internalRaceDuration;
			if( isRaceFinished() ) returnToLobby();
		}
	}

    [Server]
    public bool isRaceFinished()
    {
		bool raceFinished = true;
        for (int i = 0; i < players.Count; i++)
		{
			if( !players[i].playerCrossedFinishLine ) return false;
		}
		return raceFinished;
    }

	[Server]
	void returnToLobby()
	{
        for (int i = 0; i < players.Count; i++)
		{
	    	players[i].RpcGameOver (netId, name);
		}
		players.Clear();
		Invoke ("BackToLobby", 7f);
	}

    void BackToLobby()
    {
        FindObjectOfType<NetworkLobbyManager> ().ServerReturnToLobby ();
    }

	void StartRunningEvent()
	{
		Debug.Log("Multiplayer - Player: received StartRunningEvent");
		raceStarted = true;
	}

	[Client]
	void OnRaceDurationChanged( float value )
    {
		raceDuration = value;
		PlayerRaceManager.Instance.raceDuration = raceDuration;
		Debug.Log("OnRaceDurationChanged: " + value );
    }

	[Client]
	void OnAnimationTriggerChanged( int animationTrigger )
    {
		syncAnimationTrigger = animationTrigger;
		if( !isLocalPlayer ) GetComponent<Animator>().SetTrigger( animationTrigger );
    }

	[Command]
	public void CmdProvideAnimationTriggerToServer( int animationTrigger )
	{
		syncAnimationTrigger = animationTrigger;
	}



}