﻿using UnityEngine;
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
	}
	#endregion

	#region Public Methods
	//startMatch is called by MatchmakingManager when the Play button is clicked
	public void startMatch()
	{
		Debug.Log("startMatch " + GameManager.Instance.getPlayMode() );

		//Does the selected play mode require Internet?
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy )
		{
			//PlayAgainstEnemy is an offline mode. We do not need to check that we have access to the Internet.
			connecting = true;
			//Do not call PhotonNetwork.ConnectUsingSettings(...) as it does not make sense when playing offline.
			//The method tryToJoinRoom will behave as expected even when not connected.
			tryToJoinRoom();
			setUpBot();
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
		PhotonNetwork.playerName = PlayerStatsManager.Instance.getUserName();

		//Set the index of the selected hero so we can retrieve it later. Hero data is stored in HeroManager.
		playerCustomProperties.Add("Hero", LevelManager.Instance.selectedHeroIndex );
		
		//Set your icon, which is displayed in the matchmaking screen
		playerCustomProperties.Add("Icon", GameManager.Instance.playerProfile.getPlayerIconId() );

		//Set your level, which determines the frame to use in the matchmaking screen
		playerCustomProperties.Add("Level", GameManager.Instance.playerProfile.getLevel() );

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

	    	setConnectionProgressOnTryToJoinRoom();
	
			//Join the selected circuit such as CIRUIT_PRACTICE_RUN.
			LevelData.CircuitInfo circuitInfo = LevelManager.Instance.getSelectedCircuitInfo();
			Debug.Log("MPNetworkLobbyManager-tryToJoinRoom-The circuit selected by the player is: " + circuitInfo.matchName );
	
			//In addition, join a match that corresponds to the player's elo rating.
			int playerEloRating = ProgressionManager.Instance.getEloRating( GameManager.Instance.playerProfile.getLevel() );
	
			//Try to join an existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
			ExitGames.Client.Photon.Hashtable desiredRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", circuitInfo.matchName }, { "Elo", playerEloRating } };
			PhotonNetwork.JoinRandomRoom( desiredRoomProperties, LevelManager.Instance.getNumberOfPlayersRequired() );
		}
	}	 

 	void setConnectionProgressOnTryToJoinRoom()
	{
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstEnemy:
				matchmakingManager.setConnectionProgress( "Setting up Player vs. Enemy race" );   
			break;

			case PlayMode.PlayAlone:
				matchmakingManager.hideRemotePlayer();
				matchmakingManager.setConnectionProgress( "Playing alone" );   
			break;

			case PlayMode.PlayOthers:
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
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Fake searching for a player for a few seconds ...
				Invoke( "displayBotInfo", DELAY_BEFORE_DISPLAY_BOT );
				//Note: In this case, LoadArena() gets called by displayBotInfo
			break;

			case PlayMode.PlayAlone:
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
				//PlayerPosition 3 is the center lane.
				playerCustomProperties.Add("PlayerPosition", 3 );
				PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
				//Since we want to play alone for testing purposes, load level right away.
		        PhotonNetwork.LoadLevel("Level");
				//Note: In this case, LoadArena() never gets called. We load the level directly.
			break;

			case PlayMode.PlayOthers:
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
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
				LevelManager.Instance.setNumberOfPlayersRequired( 3 );
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
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
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
		matchmakingManager.setConnectionProgress( "Traveling to " + LocalizationManager.Instance.getText( LevelManager.Instance.getSelectedMultiplayerLevel().circuitInfo.circuitTextID ) + " ..." ); 
		matchmakingManager.setRemotePlayerData( numberRemotePlayerConnected, player.NickName, (int)player.CustomProperties["Level"], (int)player.CustomProperties["Icon"] );
	}

	void setUpBot()
	{
		HeroManager.HeroCharacter selectedHero = HeroManager.Instance.getHeroCharacter( LevelManager.Instance.selectedHeroIndex );
		int botHeroIndex = HeroManager.Instance.getIndexOfOppositeSexBot( selectedHero.sex );
		LevelManager.Instance.selectedBotHeroIndex = botHeroIndex;
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( botHeroIndex );
	
	}

	void displayBotInfo()
	{
		matchmakingManager.disableExitButton();
		matchmakingManager.setConnectionProgress( "Traveling to " + LocalizationManager.Instance.getText( LevelManager.Instance.getSelectedMultiplayerLevel().circuitInfo.circuitTextID ) + " ..." ); 
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
		matchmakingManager.setRemotePlayerData( 1, botHero.userName, botHero.level, botHero.playerIcon );
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
				//Now that we have everyone and we can start the race, deduct the entry fee, if any.
				chargePlayerForMatch();
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
