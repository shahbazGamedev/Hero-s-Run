using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;

public class MPNetworkLobbyManager : NetworkLobbyManager 
{
	public static MPNetworkLobbyManager mpNetworkLobbyManager;
	public MPLobbyMenu mpLobbyMenu;
	public MatchInfo hostedMatchInfo = null;
	public MatchInfo joinedMatchInfo = null;
	public int lobbyPlayerCount = 0;
	public int levelPlayerCount = 0;
	public int minimumPlayersToStartMatch = 2;
	bool levelLoading = false;
	HUDMultiplayer hudMultiplayer;
	bool startedCountdown = false;
	//In the Network Manager component, you must put your player prefabs 
    //in the Spawn Info -> Registered Spawnable Prefabs section 
	public Dictionary<int,int> dico = new Dictionary<int,int>();
	public class MsgTypes
	{
		public const short PlayerPrefab = MsgType.Highest + 1;
		
		public class PlayerPrefabMsg : MessageBase
		{
			public short playerControllerId;
			public int connectionId;    
			public int prefabIndex;
			public string playerName;
		}
	}

	void Start()
	{
		minimumPlayersToStartMatch = LevelManager.Instance.getNumberOfPlayersRequired();
		Debug.Log("\nENTERING MULTIPLAYER " + minimumPlayersToStartMatch );
	    mpNetworkLobbyManager = this;
	    DontDestroyOnLoad(gameObject);
	}

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler(MsgTypes.PlayerPrefab, OnResponsePrefab);
		base.OnStartServer();
	}

	public override void OnStopServer()
	{
		NetworkServer.UnregisterHandler(MsgTypes.PlayerPrefab);
		base.OnStopServer();
	}

	//On server
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		Debug.Log("OnServerAddPlayer: conn.connectionId: " + conn.connectionId + " playerControllerId: " + playerControllerId );

		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.playerControllerId = playerControllerId;
		msg.connectionId = conn.connectionId;

		//Ask the player that just got added his player information such as his name and his prefab index
		NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefab, msg);
	}

	//On client
	private void OnRequestPrefab(NetworkMessage netMsg)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		//Recopy this info
		MsgTypes.PlayerPrefabMsg serverMsg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
		msg.connectionId = serverMsg.connectionId;
		msg.playerControllerId = serverMsg.playerControllerId;
		//Now copy the player info requested
		msg.prefabIndex = (int) PlayerStatsManager.Instance.getAvatar();
		msg.playerName = PlayerStatsManager.Instance.getUserName();
		
		//The server just asked us about our player information, so tell him
		client.Send(MsgTypes.PlayerPrefab, msg);
		Debug.Log("OnRequestPrefab: " + msg.connectionId + " " + msg.playerName  );
	}
	
	//On server
	private void OnResponsePrefab(NetworkMessage netMsg)
	{
		//The nice client just told us what his player information is
		MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>(); 
		Debug.Log("OnResponsePrefab: " + msg.connectionId + " " + msg.playerName + " Prefab name " + gamePlayerPrefab.name );
		dico.Add( msg.connectionId, msg.prefabIndex );
		base.OnServerAddPlayer(netMsg.conn, msg.playerControllerId);
	}

	public void startMatch()
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			StartMatchMaker();
			//Ask for a list of matches. This will determine if we need to create a match or not.
			matchMaker.ListMatches( 0, 20, "", false, 0, 0, OnMatchList );
		}
		else
		{
			mpLobbyMenu.showNoInternetPopup();
		}
	}

	void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchInfoSnapshotList )
	{

		if( success )
		{
			if( matchInfoSnapshotList.Count == 0)
			{
				Debug.Log("MPNetworkLobbyManager-OnMatchList: No matches found!" );
				//We will therefore create a match. By doing so, we will become the host of the game.
				//When creating a match, you automatically join it.
				matchMaker.CreateMatch( "Match Name 1", (uint)2, true, "", "", "", 0, 0, OnMatchCreate );
			}
			else
			{
				Debug.Log("MPNetworkLobbyManager-OnMatchList: Matches found!" );
				//We found some matches. Let's join the first match that is not full on the list.
				MatchInfoSnapshot matchToJoin = null;
				Debug.Log("List of matches\n");
				for( int i = 0; i < matchInfoSnapshotList.Count; i++ )
				{
					if( matchInfoSnapshotList[i].currentSize < matchInfoSnapshotList[i].maxSize )
					{
						matchToJoin = matchInfoSnapshotList[i];
						break;
					}
				}
				if( matchToJoin == null )
				{
					Debug.LogWarning("MPNetworkLobbyManager-OnMatchList: All matches found are full." );
				}
				else
				{
					matchMaker.JoinMatch(matchToJoin.networkId, "","","",0,0, OnMatchJoined );
				}
			}
		}
		else
		{
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchList: Error: " + extendedInfo );
			if( extendedInfo.Contains("Connection time-out") ) mpLobbyMenu.showConnectionTimedOut();
		}
	}
	
	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		base.OnMatchCreate(success, extendedInfo, matchInfo);
		if( success )
		{
			Debug.Log("MPNetworkLobbyManager-OnMatchCreate: Success" );
			hostedMatchInfo = matchInfo;
		}
		else
		{
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchCreate: Error: " + extendedInfo );
			mpLobbyMenu.showUnableToCreateMatch();
		}
	}

	public override void OnLobbyClientEnter()
	{
		Debug.Log("MPNetworkLobbyManager-OnLobbyClientEnter");
		base.OnLobbyClientEnter();
	}

	public override void OnLobbyServerPlayersReady()
	{
		//Do we have enough players to start the match?
		if( lobbyPlayerCount >=  minimumPlayersToStartMatch )
		{
			bool allready = true;
			for(int i = 0; i < lobbySlots.Length; ++i)
			{
				if(lobbySlots[i] != null)
				allready &= lobbySlots[i].readyToBegin;
			}	
			if(allready)
			{
				Debug.Log("MPNetworkLobbyManager-OnLobbyServerPlayersReady: Loading Play Scene\n");
				ServerChangeScene(playScene);
			}
		}
		else
		{
			Debug.Log("MPNetworkLobbyManager-OnLobbyServerPlayersReady: We have " + lobbyPlayerCount + " players, but we need " + minimumPlayersToStartMatch );
		}
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		int id = -10;
		for(int i = 0; i < lobbySlots.Length; i++ )
		{
			if( lobbySlots[i] == lobbyPlayer.GetComponent<NetworkLobbyPlayer>() )
			{
				id =lobbySlots[i].connectionToClient.connectionId;
			}
		}

		gamePlayer.GetComponent<Player>().setSkin(dico[id]);
		gamePlayer.GetComponent<Player>().setPlayerName(dico[id].ToString() );
		Debug.Log("MPNetworkLobbyManager-OnLobbyServerSceneLoadedForPlayer" );
		//This hook allows you to apply state data from the lobby-player to the game-player.
		GetComponent<MPLobbyHook>().OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);
		//Do we have enough players to start the match?
		if( lobbyPlayerCount >=  minimumPlayersToStartMatch )
		{
			Debug.Log("MPNetworkLobbyManager-We have everyone." );
			if( !startedCountdown ) Invoke("startCountdown", 3f );
			startedCountdown = true;
		}
		return true;
	}

	public override void OnClientSceneChanged( NetworkConnection conn )
	{
		base.OnClientSceneChanged( conn );
		Debug.Log("MPNetworkLobbyManager-OnClientSceneChanged " + SceneManager.GetActiveScene().name + " levelPlayerCount " + levelPlayerCount );
	}

	void startCountdown()
	{
		StartCoroutine( ServerCountdownCoroutine() );
	}

	IEnumerator ServerCountdownCoroutine()
	{
		//For the 3,2,1 countdown
		int countdown = 3;
		while (countdown >= 0)
		{
			for (int i = 0; i < lobbySlots.Length; ++i)
			{
				if (lobbySlots[i] != null)
				{
					(lobbySlots[i] as MPLobbyPlayer).RpcUpdateCountdown(countdown);
				}
			}
			yield return new WaitForSecondsRealtime( 1.0f );
			countdown --;
		}
	}

	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		base.OnMatchJoined(success, extendedInfo, matchInfo);
		if( success )
		{
			Debug.Log("MPNetworkLobbyManager-OnMatchJoined: Success" );
			joinedMatchInfo = matchInfo;
		}
		else
		{
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchJoined: Error: " + extendedInfo );
			mpLobbyMenu.showUnableToJoinMatch();
		}
	}

	public void cleanUpOnExit()
	{
		if( hostedMatchInfo != null )
		{
			matchMaker.DestroyMatch( hostedMatchInfo.networkId, 0, OnDestroyMatch );
			StopHost();
			hostedMatchInfo = null;
			lobbyPlayerCount = 0;
		}
		else if( joinedMatchInfo != null )
		{
			matchMaker.DropConnection(joinedMatchInfo.networkId, joinedMatchInfo.nodeId, 0, OnDropConnection );
			joinedMatchInfo = null;
			if( lobbyPlayerCount > 0 )lobbyPlayerCount--;
		}
		levelPlayerCount = 0;
		startedCountdown = false;
		dico.Clear();
	}

	public override void OnDestroyMatch(bool success, string extendedInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnDestroyMatch: Success: " + success);
		base.OnDestroyMatch(success, extendedInfo);
		StopMatchMaker();
	}

	public override void OnDropConnection(bool success, string extendedInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnDropConnection: Success: " + success);
		base.OnDropConnection(success, extendedInfo);
		StopMatchMaker();
	}

	//Error management
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		Debug.LogWarning("MPNetworkLobbyManager-OnServerDisconnect");
		cleanUpOnExit();
	}

	// Client
	public override void OnClientConnect(NetworkConnection conn)
	{
		client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
		base.OnClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		client.UnregisterHandler(MsgTypes.PlayerPrefab);
		base.OnClientDisconnect(conn);
		Debug.LogWarning("MPNetworkLobbyManager-OnClientDisconnect");
	}
	
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		base.OnClientError( conn, errorCode );
		Debug.LogWarning("MPNetworkLobbyManager-OnClientError: Error Code: " + errorCode);
	}

	//This is called after StopHost() is called
	public override void ServerChangeScene (string sceneName)
	{
		if( sceneName == "Level" )
		{
			base.ServerChangeScene( sceneName );
		}
		else if( sceneName == "MP Lobby" )
		{
			StartCoroutine( returnToLobby() );
		}
		else if( sceneName == null )
		{
			Debug.Log("MPNetworkLobbyManager-ServerChangeScene: sceneName is null" );
			StartCoroutine( returnToLobby() );
		}
	}

	IEnumerator returnToLobby()
	{
		if( !levelLoading )
		{
			Debug.Log("MPNetworkLobbyManager-returnToLobby");
			levelLoading = true;
			GameManager.Instance.setMultiplayerMode( false );
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.MultiplayerMatchmaking );
		}
	}	

}
