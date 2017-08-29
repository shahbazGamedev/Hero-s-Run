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
	const float DELAY_BEFORE_LOADING_LEVEL = 3f;
	const float DELAY_BEFORE_DISPLAY_BOT = 2.5f;
	HUDMultiplayer hudMultiplayer;
	bool connecting = false; //true if the player pressed the Play button (which calls startMatch). This bool is to prevent rejoining a room automatically after a race.
	int numberRemotePlayerConnected = 0;
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
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayWithFriends )
		{
			matchmakingManager.setRemotePlayerData( 1, LevelManager.Instance.matchData.sender, LevelManager.Instance.matchData.level, LevelManager.Instance.matchData.playerIcon );
			ChatMessageHandler.MatchData md = LevelManager.Instance.matchData;
			PlayerFriends.FriendData playerData = new PlayerFriends.FriendData(  md.sender,  md.playerIcon,  md.level, md.currentWinStreak );
			GameManager.Instance.recentPlayers.addRecentPlayer( playerData );
		}
	}
	#endregion

	#region Public Methods
	//startMatch is called by MatchmakingManager when the Play button is clicked
	public void startMatch()
	{
		Debug.Log("startMatch " + GameManager.Instance.getPlayMode() );

		//Does the selected play mode require Internet?
		if( !GameManager.Instance.isOnlinePlayMode() )
		{
			//PlayAgainstEnemy, PlayAgainstTwoEnemies and PlayAlone are offline modes. We do not need to check that we have access to the Internet.
			connecting = true;
			//Do not call PhotonNetwork.ConnectUsingSettings(...) as it does not make sense when playing offline.
			//The method tryToJoinRoom will behave as expected even when not connected.
			tryToJoinRoom();
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy )
			{
				setUpBot();
			}
			else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoEnemies )
			{
				setUpBots();
			}
		}
		else
		{
			//Yes, we need Internet
			if( Application.internetReachability != NetworkReachability.NotReachable )
			{
				//Player is connected to the Internet
				connecting = true;
				Connect();
			}
			else
			{
				//Player is not connected to the Internet
				matchmakingManager.showNoInternetPopup();
			}
		}
	}

	void configureStaticPlayerData()
	{
		//Clear Hashtable
		playerCustomProperties.Clear();
		
		//Set player name
		PhotonNetwork.playerName = GameManager.Instance.playerProfile.getUserName();

		//Set the index of the selected hero so we can retrieve it later. Hero data is stored in HeroManager.
		playerCustomProperties.Add("Hero", GameManager.Instance.playerProfile.selectedHeroIndex );
		
		//Set your icon, which is displayed in the matchmaking screen
		playerCustomProperties.Add("Icon", GameManager.Instance.playerProfile.getPlayerIconId() );

		//Set your level, which determines the frame to use in the matchmaking screen
		playerCustomProperties.Add("Level", GameManager.Instance.playerProfile.getLevel() );

		//Set your win streak, which is displayed in the social screen
		playerCustomProperties.Add("WinStreak", GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.CURRENT_WIN_STREAK) );

		//Set trophies. This is needed to determine trophies won/lost since the value depends on the differences in trophies between the players.
		//As an example, a player with 400 trophies winning against a player with 800 trophies, will earn 50 trophies, whereas a
		//player with 600 trophies winning against a player with 800 trophies, will earn 40 trophies.
		playerCustomProperties.Add("Trophies", GameManager.Instance.playerProfile.getTrophies() );

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
		if (PhotonNetwork.connectedAndReady)
		{
			tryToJoinRoom();
			matchmakingManager.setPhotonCloudRegionText( PhotonNetwork.CloudRegion.ToString() );
		}
		else if (PhotonNetwork.connecting)
		{
			matchmakingManager.setConnectionProgress( "Connecting ..." );				
		}
		else
		{
			//We must first and foremost connect to Photon Online Server.
			//Users are separated from each other by game version (which allows you to make breaking changes).
			//In PhotonServerSettings, Hosting is set to Best Region excluding South Korea, Asia and Japan
			if( GameManager.Instance.playerDebugConfiguration.getOverrideCloudRegionCode() != CloudRegionCode.none ) PhotonNetwork.OverrideBestCloudServer( GameManager.Instance.playerDebugConfiguration.getOverrideCloudRegionCode() );
			PhotonNetwork.ConnectUsingSettings(GameManager.Instance.getVersionNumber());
			matchmakingManager.setConnectionProgress( "Connecting ..." );   
		}
	}

 	void tryToJoinRoom()
	{
		if( connecting )
		{
			configureStaticPlayerData();

	    	setConnectionProgressOnTryToJoinRoom();

			if( GameManager.Instance.getPlayMode() == PlayMode.PlayWithFriends )
			{
				LevelManager.Instance.setSelectedCircuit( LevelManager.Instance.getLevelData().getRaceTrackByName( LevelManager.Instance.matchData.raceTrackName ) );
				RoomOptions roomOptions = new RoomOptions();
				roomOptions.MaxPlayers = LevelManager.Instance.getNumberOfPlayersRequired();
				roomOptions.IsVisible = false;
			    Debug.Log("tryToJoinRoom: with friends " + LevelManager.Instance.matchData.roomName + " Max " +  LevelManager.Instance.getNumberOfPlayersRequired() );
				PhotonNetwork.JoinOrCreateRoom( LevelManager.Instance.matchData.roomName, roomOptions, TypedLobby.Default );
			}
			else
			{
				LevelData.CircuitInfo selectedCircuit = LevelManager.Instance.getSelectedCircuit().circuitInfo;
				//Join the selected circuit such as CIRUIT_PRACTICE_RUN.
				Debug.Log("MPNetworkLobbyManager-tryToJoinRoom-The circuit selected by the player is: " + selectedCircuit.raceTrackName );
		
				//In addition, join a match that corresponds to the player's elo rating.
				int playerEloRating = ProgressionManager.Instance.getEloRating( GameManager.Instance.playerProfile.getLevel() );
		
				//Try to join an existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
				ExitGames.Client.Photon.Hashtable desiredRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", selectedCircuit.raceTrackName }, { "Elo", playerEloRating } };
				PhotonNetwork.JoinRandomRoom( desiredRoomProperties, LevelManager.Instance.getNumberOfPlayersRequired() );
			}
		}
	}	 

 	void setConnectionProgressOnTryToJoinRoom()
	{
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstEnemy:
				matchmakingManager.setConnectionProgress( "Setting up Player vs. AI race" );   
			break;

			case PlayMode.PlayAgainstTwoEnemies:
				matchmakingManager.setConnectionProgress( "Setting up Player vs. Two AI race" );   
			break;

			case PlayMode.PlayAlone:
				matchmakingManager.setConnectionProgress( "Playing alone" );   
			break;

			case PlayMode.PlayTwoPlayers:
				matchmakingManager.setConnectionProgress( "Looking for worthy opponent ..." );   
			break;

			case PlayMode.PlayThreePlayers:
				matchmakingManager.setConnectionProgress( "Looking for worthy opponents ..." );   
			break;

			case PlayMode.PlayWithFriends:
				matchmakingManager.setConnectionProgress( "Waiting for friend to join ..." );   
			break;

		}
	}

	#endregion
 
 	#region Photon.PunBehaviour CallBacks
	public override void OnConnectedToMaster()
	{
		//First we try to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
		Debug.Log("MPNetworkLobbyManager: OnConnectedToMaster-Photon Cloud Region is " + PhotonNetwork.CloudRegion.ToString() );
		tryToJoinRoom();	 
		matchmakingManager.setPhotonCloudRegionText( PhotonNetwork.CloudRegion.ToString() );
	}

	public override void OnDisconnectedFromPhoton()
	{
	    Debug.LogWarning("MPNetworkLobbyManager: OnDisconnectedFromPhoton() was called by PUN");  
		matchmakingManager.setConnectionProgress( "Disconnected ..." );   
		matchmakingManager.setPhotonCloudRegionText( "N/A" );
	}
	 
	public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
	{
	    Debug.Log("MPNetworkLobbyManager:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.");
	    //We failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		int playerEloRating = ProgressionManager.Instance.getEloRating( GameManager.Instance.playerProfile.getLevel() );
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", LevelManager.Instance.getSelectedCircuit().circuitInfo.raceTrackName }, { "Elo", playerEloRating } };
		roomOptions.MaxPlayers = LevelManager.Instance.getNumberOfPlayersRequired();
		//Mandatory - you must also set customRoomPropertiesForLobby
		//With customRoomPropertiesForLobby, you define which key-values are relevant for matchmaking.
		string[] customRoomPropertiesForLobbyStringArray = new string[2];
		customRoomPropertiesForLobbyStringArray[0] = "Track";
		customRoomPropertiesForLobbyStringArray[1] = "Elo";
		roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobbyStringArray;
		//In CreateRoom, do not specify a match name. Let the server assign a random name.
	    PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
	}
	 
	public override void OnJoinedRoom()
	{
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstEnemy:
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Fake searching for a player for a few seconds ...
				Invoke( "displayBotInfo", DELAY_BEFORE_DISPLAY_BOT );
				//Note: In this case, LoadArena() gets called by displayBotInfo
			break;

			case PlayMode.PlayAgainstTwoEnemies:
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Fake searching for players for a few seconds ...
				Invoke( "displayBotsInfoPart1", DELAY_BEFORE_DISPLAY_BOT );
				//Note: In this case, LoadArena() gets called by displayBotsInfoPart2
			break;

			case PlayMode.PlayAlone:
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Since we want to play alone for testing purposes, load level right away.
		        PhotonNetwork.LoadLevel("Level");
				//Note: In this case, LoadArena() never gets called. We load the level directly.
			break;

			case PlayMode.PlayTwoPlayers:
				foreach(PhotonPlayer player in PhotonNetwork.playerList)
				{
					if( !player.IsLocal )
					{
						OnRemotePlayerConnect( player );
					}
				}
				//PlayerPosition will be used to determine the start position. We don't want players to spawn on top of each other.
				//PlayerPosition 1 is the left lane. PlayerPosition 2 is the right lane.
				playerCustomProperties.Add("PlayerPosition", PhotonNetwork.room.PlayerCount );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Note: In this case, LoadArena() gets called when the remote player connects
			break;

			case PlayMode.PlayThreePlayers:
				foreach(PhotonPlayer player in PhotonNetwork.playerList)
				{
					if( !player.IsLocal )
					{
						OnRemotePlayerConnect( player );
					}
				}
				//PlayerPosition will be used to determine the start position. We don't want players to spawn on top of each other.
				//PlayerPosition 1 is the left lane. PlayerPosition 2 is the right lane.
				playerCustomProperties.Add("PlayerPosition", PhotonNetwork.room.PlayerCount );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Note: In this case, LoadArena() gets called when the remote player connects
			break;

			case PlayMode.PlayWithFriends:
				foreach(PhotonPlayer player in PhotonNetwork.playerList)
				{
					if( !player.IsLocal )
					{
						OnRemotePlayerConnect( player );
					}
				}
				//PlayerPosition will be used to determine the start position. We don't want players to spawn on top of each other.
				//PlayerPosition 1 is the left lane. PlayerPosition 2 is the right lane.
				playerCustomProperties.Add("PlayerPosition", PhotonNetwork.room.PlayerCount );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Note: In this case, LoadArena() gets called when the remote player connects
			break;
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
	    if ( PhotonNetwork.isMasterClient ) 
	    {
			LoadArena();		 
	    }
	}

	void OnRemotePlayerConnect( PhotonPlayer player )
	{
		numberRemotePlayerConnected++;
		matchmakingManager.disableExitButton();
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + LevelManager.Instance.getSelectedCircuit().circuitInfo.sectorNumber.ToString() );
		matchmakingManager.setConnectionProgress( "Traveling to " + sectorName + " ..." ); 
		matchmakingManager.setRemotePlayerData( numberRemotePlayerConnected, player.NickName, (int)player.CustomProperties["Level"], (int)player.CustomProperties["Icon"] );
		PlayerRaceManager.Instance.setTrophiesOwnedByOpponent( numberRemotePlayerConnected, (int)player.CustomProperties["Trophies"] );
		PlayerFriends.FriendData playerData = new PlayerFriends.FriendData( player.NickName, (int)player.CustomProperties["Icon"], (int)player.CustomProperties["Level"], (int)player.CustomProperties["WinStreak"] );		
		GameManager.Instance.recentPlayers.addRecentPlayer( playerData );
	}

	void setUpBot()
	{
		HeroManager.HeroCharacter selectedHero = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex );
		int botHeroIndex = HeroManager.Instance.getIndexOfOppositeSexBot( selectedHero.sex );
		LevelManager.Instance.selectedBotHeroIndex = botHeroIndex;
	}

	void setUpBots()
	{
		LevelManager.Instance.selectedBotHeroIndex = 0;
		LevelManager.Instance.selectedBotHeroIndex2 = 1;
	}

	void displayBotInfo()
	{
		matchmakingManager.disableExitButton();
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + LevelManager.Instance.getSelectedCircuit().circuitInfo.sectorNumber.ToString() );
		matchmakingManager.setConnectionProgress( "Traveling to " + sectorName + " ..." );
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
		matchmakingManager.setRemotePlayerData( 1, botHero.userName, botHero.level, botHero.playerIcon );
		LoadArena();
	}

	void displayBotsInfoPart1()
	{
		matchmakingManager.disableExitButton();
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + LevelManager.Instance.getSelectedCircuit().circuitInfo.sectorNumber.ToString() );
		matchmakingManager.setConnectionProgress( "Traveling to " + sectorName + " ..." );
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
		matchmakingManager.setRemotePlayerData( 1, botHero.userName, botHero.level, botHero.playerIcon );

		Invoke("displayBotsInfoPart2", Random.Range(0.4f, 0.9f ) );
	}

	void displayBotsInfoPart2()
	{
		HeroManager.BotHeroCharacter botHero2 = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex2 );
		matchmakingManager.setRemotePlayerData( 2, botHero2.userName, botHero2.level, botHero2.playerIcon );

		LoadArena();
	}

	public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
	{
		Debug.Log( "MPNetworkLobbyManager: OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
		matchmakingManager.setConnectionProgress( other.NickName + " just disconnected." );   
	}

	void LoadArena()
	{
	    if ( PhotonNetwork.isMasterClient ) 
	    {
			if( PhotonNetwork.room.PlayerCount == LevelManager.Instance.getNumberOfPlayersRequired() )
			{
				//Close the room. We do not want people to join while a race is in progress.
				PhotonNetwork.room.IsOpen = false;
		   	 	Invoke("loadLevel", DELAY_BEFORE_LOADING_LEVEL);
			}
		}
		else
		{
	        Debug.LogError( "MPNetworkLobbyManager: Trying to Load a level but we are not the master Client" );
		}
	}
	 
	void loadLevel()
	{
		PhotonNetwork.LoadLevel("Level");
	}
	#endregion

}
