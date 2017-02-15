using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Photon;
using ExitGames.Client.Photon;

public class MPNetworkLobbyManager : PunBehaviour 
{
	#region Public Variables
	public static MPNetworkLobbyManager Instance;
	MatchmakingManager matchmakingManager;
	[SerializeField] PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
	#endregion

	#region Private Variables
	ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
	//The delay to wait before loading the level. We have this delay so that the player can see the opponent's name and icon for
	//a few seconds before the match begins.
	const float DELAY_BEFORE_LOADING_LEVEL = 5f;
	byte numberOfPlayersRequired = 2;
	HUDMultiplayer hudMultiplayer;
	bool connecting = false; //true if the player pressed the Play button (which calls startMatch). This bool is to prevent rejoining a room automatically after a race.
	#endregion

	#region MonoBehaviour CallBacks
	void Awake()
	{
		Instance = this;

		PhotonNetwork.logLevel = Loglevel;
		
		// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
		PhotonNetwork.autoJoinLobby = false;
		
		// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
		PhotonNetwork.automaticallySyncScene = true;
	}

	void Start()
	{
		matchmakingManager = GameObject.FindGameObjectWithTag("Matchmaking").GetComponent<MatchmakingManager>();
		numberOfPlayersRequired = LevelManager.Instance.getNumberOfPlayersRequired();
	}
	#endregion

	#region Public Methods	
	public void startMatch()
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			connecting = true;
			Connect();
		}
		else
		{
			matchmakingManager.showNoInternetPopup();
		}
	}

	void configureStaticPlayerData()
	{
		//Clear Hashtable
		playerCustomProperties.Clear();
		
		//Set player name
		PhotonNetwork.playerName = PlayerStatsManager.Instance.getUserName();

		//Set the player's skin
		if( PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
		{
			playerCustomProperties.Add("Skin", "Hero_prefab" );
		}
		else
		{
			playerCustomProperties.Add("Skin", "Heroine_prefab" );
		}
	
		//Set your icon, which is displayed in the matchmaking screen
		playerCustomProperties.Add("Icon", GameManager.Instance.playerProfile.getPlayerIconId() );

		//Apply values so that they propagate to other players
		PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
	}

	/// <summary>
	/// Start the connection process. 
	/// - If already connected, we attempt joining a random room
	/// - if not yet connected, Connect this application instance to Photon Cloud Network
	/// </summary>
	void Connect()
	{
		// we check if we are connected or not. We join if we are, else we initiate the connection to the server.
		if (PhotonNetwork.connected)
		{
			tryToJoinRoom();
		}
		else
		{
			//We must first and foremost connect to Photon Online Server.
			//Users are separated from each other by game version (which allows you to make breaking changes).
			PhotonNetwork.ConnectUsingSettings(GameManager.Instance.getVersionNumber());
			matchmakingManager.setConnectionProgress( "Connecting ..." );   
		}
	}

 	void tryToJoinRoom()
	{
		if( connecting )
		{
			configureStaticPlayerData();

	    	if ( numberOfPlayersRequired == 1 )
		    {
				matchmakingManager.setConnectionProgress( "Connected. Playing single-player." );   
			}
			else
			{
				matchmakingManager.setConnectionProgress( "Connected. Now looking for worthy opponent ..." );   
			}
	
			//Join the selected circuit such as CIRUIT_PRACTICE_RUN.
			LevelData.CircuitInfo circuitInfo = LevelManager.Instance.getSelectedCircuitInfo();
			Debug.Log("MPNetworkLobbyManager-tryToJoinRoom-The circuit selected by the player is: " + circuitInfo.matchName );
	
			//In addition, join a match that corresponds to the player's elo rating.
			int playerEloRating = ProgressionManager.Instance.getEloRating( GameManager.Instance.playerProfile.getLevel() );
	
			//Try to join an existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
			ExitGames.Client.Photon.Hashtable desiredRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", circuitInfo.matchName }, { "Elo", playerEloRating } };
			PhotonNetwork.JoinRandomRoom( desiredRoomProperties, numberOfPlayersRequired );
		}
	}	 

	#endregion
 
 	#region Photon.PunBehaviour CallBacks
	public override void OnConnectedToMaster()
	{
		//First we try to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
		tryToJoinRoom();	 
	}

	public override void OnDisconnectedFromPhoton()
	{
	    Debug.LogWarning("MPNetworkLobbyManager: OnDisconnectedFromPhoton() was called by PUN");  
		matchmakingManager.setConnectionProgress( "Disconnected ..." );   
	}
	 
	public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
	{
	    Debug.Log("MPNetworkLobbyManager:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.");
	 
	    //We failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		int playerEloRating = ProgressionManager.Instance.getEloRating( GameManager.Instance.playerProfile.getLevel() );
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", LevelManager.Instance.getSelectedCircuitInfo().matchName }, { "Elo", playerEloRating } };
		roomOptions.MaxPlayers = numberOfPlayersRequired;
		//Mandatory - you must also set customRoomPropertiesForLobby
		//With customRoomPropertiesForLobby, you define which key-values are relevant for matchmaking.
		string[] customRoomPropertiesForLobbyStringArray = new string[2];
		customRoomPropertiesForLobbyStringArray[0] = "Track";
		customRoomPropertiesForLobbyStringArray[1] = "Elo";
		roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobbyStringArray;
		//In CreateRoom, do not specify a match name. Let the server assign a random name.
	    PhotonNetwork.CreateRoom(null, roomOptions, null);
	}
	 
	public override void OnJoinedRoom()
	{
		Debug.Log("MPNetworkLobbyManager: OnJoinedRoom() called by PUN. Now this client is in a room. Elo rating is:" + PhotonNetwork.room.CustomProperties["Elo"].ToString() + " Circuit is: " + PhotonNetwork.room.CustomProperties["Track"].ToString() + " SKin is " + PlayerStatsManager.Instance.getAvatar().ToString() );
		foreach(PhotonPlayer player in PhotonNetwork.playerList)
		{
			if( !player.IsLocal )
			{
				OnRemotePlayerConnect( player );
			}
		}

		//PlayerPosition will be used to determine the start position. We don't want players to spawn on top of each other.
		playerCustomProperties.Add("PlayerPosition", PhotonNetwork.room.PlayerCount );
		PhotonNetwork.player.SetCustomProperties(playerCustomProperties);

		//Since we want to play alone for testing purposes, load level right away.
	    if ( numberOfPlayersRequired == 1 )
	    {
	        PhotonNetwork.LoadLevel("Level");
		}				
	}

	public override void OnLeftRoom()
	{
		connecting = false; //we don't want to rejoin a room automatically
	}

	/// <summary>
	/// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
	/// </summary>
	/// <remarks>If your game starts with a certain number of players, this callback can be useful to check the
	/// Room.playerCount and find out if you can start.</remarks>
	/// <param name="newPlayer">New player.</param>
	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer )
	{
		Debug.Log("MPNetworkLobbyManager: OnPhotonPlayerConnected() called by PUN. Name: " + newPlayer.NickName + " isLocal: " + newPlayer.IsLocal + " PlayerCount: " + PhotonNetwork.room.PlayerCount );
		OnRemotePlayerConnect( newPlayer );
		LoadArena();		 
	}

	void OnRemotePlayerConnect( PhotonPlayer player )
	{
		matchmakingManager.disableExitButton();
		matchmakingManager.setConnectionProgress( "Traveling to " + LocalizationManager.Instance.getText( LevelManager.Instance.getSelectedMultiplayerLevel().circuitInfo.circuitTextID ) + " ..." ); 
		matchmakingManager.setRemotePlayerName( player.NickName );
		matchmakingManager.setRemotePlayerIcon( (int)player.CustomProperties["Icon"] );
	}

	public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
	{
		Debug.Log( "MPNetworkLobbyManager: OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
		matchmakingManager.setConnectionProgress( other.NickName + " just disconnected." );   
	}

	void LoadArena()
	{
	    if ( ! PhotonNetwork.isMasterClient ) 
	    {
	        Debug.LogError( "MPNetworkLobbyManager: Trying to Load a level but we are not the master Client" );
			return;
	    }
		if( PhotonNetwork.room.PlayerCount == numberOfPlayersRequired )
		{
			//Now that we have everyone and we can start the race, deduct the entry fee, if any.
			chargePlayerForMatch();

	   	 	Invoke("loadLevel", DELAY_BEFORE_LOADING_LEVEL);
		}
	}
	 
	void loadLevel()
	{
		PhotonNetwork.LoadLevel("Level");
	}

	void chargePlayerForMatch()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedMultiplayerLevel();
		//If the race is not free, deduct the entry fee and save.
		int entryFee = multiplayerInfo.circuitInfo.entryFee;
		if( entryFee > 0 )
		{
			PlayerStatsManager.Instance.modifyCurrentCoins(-entryFee, false, false );
			PlayerStatsManager.Instance.savePlayerStats();
			Debug.Log("MPNetworkLobbyManager-chargePlayerForMatch: deducting entry fee of: " + entryFee );
		}
	}
	#endregion

}
