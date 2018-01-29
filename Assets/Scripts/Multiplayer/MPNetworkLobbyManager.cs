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
	const float DELAY_BEFORE_LOADING_LEVEL = 1.75f;
	const float DELAY_BEFORE_DISPLAY_BOT = 1.5f;
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
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneFriend )
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

		//Make sure we reset the skill bonus to 0 at the beginning of every match.
		GameManager.Instance.playerProfile.resetSkillBonus();

		//Does the selected play mode require Internet?
		if( !GameManager.Instance.isOnlinePlayMode() )
		{
			//PlayAgainstEnemy, PlayAgainstTwoEnemies and PlayAlone are offline modes. We do not need to check that we have access to the Internet.
			connecting = true;
			//Do not call PhotonNetwork.ConnectUsingSettings(...) as it does not make sense when playing offline.
			//The method tryToJoinRoom will behave as expected even when not connected.
			tryToJoinRoom();
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneBot || GameManager.Instance.getPlayMode() == PlayMode.PlayCoopWithOneBot )
			{
				setUpBot();
			}
			else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoBots )
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

			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneFriend )
			{
				LevelManager.Instance.setSelectedCircuit( LevelManager.Instance.getLevelData().getMapByName( LevelManager.Instance.matchData.mapName ) );
				RoomOptions roomOptions = new RoomOptions();
				roomOptions.MaxPlayers = LevelManager.Instance.getNumberOfPlayersRequired();
				roomOptions.IsVisible = false;
			    Debug.Log("tryToJoinRoom: with friends " + LevelManager.Instance.matchData.roomName + " Max " +  LevelManager.Instance.getNumberOfPlayersRequired() );
				PhotonNetwork.JoinOrCreateRoom( LevelManager.Instance.matchData.roomName, roomOptions, TypedLobby.Default );
			}
			else
			{
				int currentSector = GameManager.Instance.playerProfile.getCurrentSector();
				LevelData.CircuitInfo selectedCircuit = LevelManager.Instance.getSelectedCircuit().circuitInfo;
				//Join the selected circuit such as CIRUIT_PRACTICE_RUN.
				Debug.Log("MPNetworkLobbyManager-tryToJoinRoom-The circuit selected by the player is: " + selectedCircuit.mapName + " for Sector " + currentSector );
		
				//Try to join an existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
				ExitGames.Client.Photon.Hashtable desiredRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Sector", currentSector } };
				PhotonNetwork.JoinRandomRoom( desiredRoomProperties, LevelManager.Instance.getNumberOfPlayersRequired() );
			}
		}
	}	 

 	void setConnectionProgressOnTryToJoinRoom()
	{
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstOneBot:
				matchmakingManager.setConnectionProgress( "Setting up Player vs. AI race" );   
			break;

			case PlayMode.PlayCoopWithOneBot:
				matchmakingManager.setConnectionProgress( "Setting up Coop with AI race" );   
			break;

			case PlayMode.PlayAgainstTwoBots:
				matchmakingManager.setConnectionProgress( "Setting up Player vs. Two AI race" );   
			break;

			case PlayMode.PlayAlone:
				matchmakingManager.setConnectionProgress( "Playing alone" );   
			break;

			case PlayMode.PlayAgainstOnePlayer:
				matchmakingManager.setConnectionProgress( "Looking for worthy opponent ..." );   
			break;

			case PlayMode.PlayCoopWithOnePlayer:
				matchmakingManager.setConnectionProgress( "Looking for worthy coop partner ..." );   
			break;

			case PlayMode.PlayAgainstTwoPlayers:
				matchmakingManager.setConnectionProgress( "Looking for worthy opponents ..." );   
			break;

			case PlayMode.PlayAgainstOneFriend:
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
		//Now make sure we re-enable the play button and the exit button in the matchmaking scene or else the player will be stuck.
		matchmakingManager.enableExitButton( true );
		matchmakingManager.enablePlayButton( true );
	}
	 
	public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
	{
		int currentSector = GameManager.Instance.playerProfile.getCurrentSector();
	    Debug.Log("MPNetworkLobbyManager:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. " + LevelManager.Instance.getSelectedCircuit().circuitInfo.mapName + " for sector " + currentSector );
	    //We failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		//In multiplayer games, we want all the players to have the same results when using the random generator. This is why we seed it.
		//The seed is set by the master when the room is created.
		RoomOptions roomOptions = new RoomOptions();
		int randomSeed = Random.Range( 0, 777777 );
		Debug.Log("Matchmaking randomSeed " + randomSeed );
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Map", LevelManager.Instance.getSelectedCircuit().circuitInfo.mapName }, { "Sector", currentSector }, { "Seed", randomSeed } };
		roomOptions.MaxPlayers = LevelManager.Instance.getNumberOfPlayersRequired();
		//Mandatory - you must also set customRoomPropertiesForLobby
		//With customRoomPropertiesForLobby, you define which key-values are relevant for matchmaking.
		string[] customRoomPropertiesForLobbyStringArray = new string[1];
		customRoomPropertiesForLobbyStringArray[0] = "Sector";
		roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobbyStringArray;
		//In CreateRoom, do not specify a match name. Let the server assign a random name.
	    PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
	}
	 
	public override void OnJoinedRoom()
	{
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstOneBot:
			case PlayMode.PlayCoopWithOneBot:
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Fake searching for a player for a few seconds ...
				Invoke( "displayBotInfo", DELAY_BEFORE_DISPLAY_BOT );
				//Note: In this case, LoadArena() gets called by displayBotInfo
			break;

			case PlayMode.PlayAgainstTwoBots:
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

			case PlayMode.PlayAgainstOnePlayer:
			case PlayMode.PlayCoopWithOnePlayer:
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

			case PlayMode.PlayAgainstTwoPlayers:
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

			case PlayMode.PlayAgainstOneFriend:
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
		matchmakingManager.enableExitButton( false );
		matchmakingManager.setRemotePlayerData( numberRemotePlayerConnected, player.NickName, (int)player.CustomProperties["Level"], (int)player.CustomProperties["Icon"] );
		PlayerRaceManager.Instance.setTrophiesOwnedByOpponent( numberRemotePlayerConnected, (int)player.CustomProperties["Trophies"] );
		PlayerFriends.FriendData playerData = new PlayerFriends.FriendData( player.NickName, (int)player.CustomProperties["Icon"], (int)player.CustomProperties["Level"], (int)player.CustomProperties["WinStreak"] );		
		GameManager.Instance.recentPlayers.addRecentPlayer( playerData );
		PlayerMatchData pmd = new PlayerMatchData( player.NickName, (int)player.CustomProperties["Icon"], (int)player.CustomProperties["Level"], (int)player.CustomProperties["WinStreak"] );
		LevelManager.Instance.playerMatchDataList.Add( pmd );
		displayMap();

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
		matchmakingManager.enableExitButton( false );
		setTravelingConnectionProgress( LevelManager.Instance.getSelectedCircuit().circuitInfo );
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
		matchmakingManager.setRemotePlayerData( 1, botHero.userName, botHero.level, botHero.playerIcon );
		PlayerMatchData pmd = new PlayerMatchData( botHero.userName, botHero.playerIcon, botHero.level, 0 );
		LevelManager.Instance.playerMatchDataList.Add( pmd );
		LoadArena();
	}

	void displayBotsInfoPart1()
	{
		matchmakingManager.enableExitButton( false );
		setTravelingConnectionProgress( LevelManager.Instance.getSelectedCircuit().circuitInfo );
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
		matchmakingManager.setRemotePlayerData( 1, botHero.userName, botHero.level, botHero.playerIcon );
		PlayerMatchData pmd = new PlayerMatchData( botHero.userName, botHero.playerIcon, botHero.level, 0 );
		LevelManager.Instance.playerMatchDataList.Add( pmd );

		Invoke("displayBotsInfoPart2", Random.Range(0.4f, 0.9f ) );
	}

	void displayBotsInfoPart2()
	{
		HeroManager.BotHeroCharacter botHero2 = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex2 );
		matchmakingManager.setRemotePlayerData( 2, botHero2.userName, botHero2.level, botHero2.playerIcon );
		PlayerMatchData pmd = new PlayerMatchData( botHero2.userName, botHero2.playerIcon, botHero2.level, 0 );
		LevelManager.Instance.playerMatchDataList.Add( pmd );

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
	
	void displayMap()
	{
		//When playing offline, the room generated by Photon will not have custom properties.
		//This is why we make sure it is not null.
		ExitGames.Client.Photon.Hashtable customRoomProperties = PhotonNetwork.room.CustomProperties;
		if( customRoomProperties != null )
		{
			if( customRoomProperties.ContainsKey("Map") )
			{
				string mapName = PhotonNetwork.room.CustomProperties["Map"].ToString();
				Debug.Log("MPNetworkLobbyManager displayMap Map " + mapName );
				LevelData.MultiplayerInfo mi = LevelManager.Instance.getLevelData().getMapByName( mapName );
				setTravelingConnectionProgress( mi.circuitInfo );
				matchmakingManager.configureCircuitData( mi.circuitInfo );
				matchmakingManager.hidePlayButton();
			}
			else
			{
				Debug.LogError("MPNetworkLobbyManager-displayMap: customRoomProperties does not contain the key Track " + PhotonNetwork.room.Name );
			}
		}
		else
		{
			Debug.LogError("MPNetworkLobbyManager-displayMap: customRoomProperties is null" );
		}
	}
 
	void setTravelingConnectionProgress( LevelData.CircuitInfo circuitInfo )
	{
		string sectorName = LocalizationManager.Instance.getText( "MAP_" + circuitInfo.mapNumber.ToString() );
		string travelingTo = string.Format( LocalizationManager.Instance.getText( "MULTI_TRAVELING_TO" ), sectorName );
		matchmakingManager.setConnectionProgress( travelingTo  ); 
	}

	void loadLevel()
	{
		PhotonNetwork.LoadLevel("Level");
	}
	#endregion

}
