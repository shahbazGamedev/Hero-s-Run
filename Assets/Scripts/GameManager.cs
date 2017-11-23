﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;



public enum GameState {
	
	Unknown = 0,
	Normal = 1,
	Paused = 2,
	Countdown = 3,
	SaveMe = 4,
	Resurrect = 5,
	Menu = 6,
	Checkpoint = 7,
	BeforeTapToPlayAllowed = 8,
	PostLevelPopup = 9,
	WorldMapNoPopup = 10,
	MultiplayerEndOfGame = 11,
	Matchmaking = 12
}

//The GameScenes enum entries must match the scene numbering in Build Settings.
//Sample usage: SceneManager.LoadScene( (int)GameScenes.CharacterSelection )
public enum GameScenes {	
	
	TitleScreen = 0,
	Level = 1,
	CircuitSelection = 2,
	HeroSelection = 3,
	Matchmaking = 4,
	CareerProfile = 5,
	MainMenu = 6,
	PlayModes = 7,
	Options = 8,
	Training = 9,
	Social = 10,
	LootBox = 11,

	//Not used
	WorldMap = 12,
	TreasureIsland = 13,
	CharacterGallery = 14,
	Journal = 15
}

public enum GameMode {
	
	Story = 1,
	Endless = 2
}

public enum PlayMode {
	
	PlayAlone = 1,
	PlayAgainstOnePlayer = 2,
	PlayAgainstTwoPlayers = 3,

	PlayAgainstOneFriend = 4,

	PlayAgainstOneBot = 6,
	PlayAgainstTwoBots = 7,

	PlayCoopWithOnePlayer = 8,
	PlayCoopWithOneBot = 9

}


//This class is a Singleton
public class GameManager {

	private GameState gameState = GameState.Unknown;
	private string versionNumber = System.String.Empty;

	private static GameManager gameManager = null;

	//Event management used to notify other classes when the game state has changed
	public delegate void GameStateEvent( GameState previousValue, GameState newValue );
	public static event GameStateEvent gameStateEvent;

	private GameMode gameMode = GameMode.Story;
	private bool multiplayerMode = true;
	public Sprite selfie;
	public byte[] selfieBytes;

	//We keep a reference to the ChallengeBoard here because we need to access it from the level scene
	public ChallengeBoard challengeBoard;
	//We keep a reference to the JournalData and JournalAssetManager here because we need to access it from multiple scenes
	public JournalData journalData;
	public JournalAssetManager journalAssetManager;
	public PlayerProfile playerProfile;
	public PlayerStatistics playerStatistics;
	public PlayerDeck playerDeck;
	public PlayerFriends playerFriends;
	public RecentPlayers recentPlayers;
	public PlayerInventory playerInventory;
	public PlayerIcons playerIcons;
	public PlayerVoiceLines playerVoiceLines;
	public PlayerConfiguration playerConfiguration;
	public PlayerDebugConfiguration playerDebugConfiguration;
	PlayMode playMode;
	//When you change the time scale (during the end of race slowdown for example), you also need to change
	//fixedDeltaTime by the same amount or else the camera will be jerky.
	//When we reset Time.scale to 1, we also need to reset fixedDeltaTime to it's original value, so let's save it.
	//@see TimeManager settings.
	public const float DEFAULT_FIXED_DELTA_TIME = 0.03333333f;

	public static GameManager Instance
	{
        get
		{
            if (gameManager == null)
			{

                gameManager = new GameManager();
            }
            return gameManager;
        }
    } 
	
	public void setGameState( GameState newState )
	{
		//Send an event to interested classes
		if(gameStateEvent != null) gameStateEvent( gameState, newState );
		gameState = newState;
		Debug.Log("GameManager-setGameState: new state is " + gameState );
	} 

	public GameState getGameState()
	{
		return gameState;
	}

	/// <summary>
	/// Sets the play mode. This also sets the number of required players in LevelManager to the correct number.
	/// </summary>
	/// <param name="playMode">Play mode.</param>
	public void setPlayMode( PlayMode playMode )
	{
		this.playMode = playMode;
		Debug.Log("GameManager-setPlayMode: new mode is " + playMode );
		switch ( playMode )
		{
			case PlayMode.PlayAgainstOneBot:
			case PlayMode.PlayAgainstTwoBots:
			case PlayMode.PlayCoopWithOneBot:
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
			break;

			case PlayMode.PlayAlone:
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
			break;

			case PlayMode.PlayAgainstOnePlayer:
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
			break;

			case PlayMode.PlayAgainstTwoPlayers:
				LevelManager.Instance.setNumberOfPlayersRequired( 3 );
			break;

			case PlayMode.PlayAgainstOneFriend:
			case PlayMode.PlayCoopWithOnePlayer:		
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
			break;
		}
		// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
		PhotonNetwork.autoJoinLobby = false;
		//Are we playing online or doing an offline PvE/solo match?
		if( getPlayMode() == PlayMode.PlayAgainstOneBot || getPlayMode() == PlayMode.PlayAgainstTwoBots || getPlayMode() == PlayMode.PlayAlone || getPlayMode()  == PlayMode.PlayCoopWithOneBot )
		{
			//PvE is an offline mode. We will not connect. We will also set Photon to offline.
			if( PhotonNetwork.connected ) PhotonNetwork.Disconnect();
			PhotonNetwork.offlineMode = true;
		}
		else
		{
			//All other play modes are online.
			PhotonNetwork.offlineMode = false;

			//Users are separated from each other by game version (which allows you to make breaking changes).
			//Don't attempt to connect if you are already connected.
			if ( !PhotonNetwork.connectedAndReady && !PhotonNetwork.connecting )
			{
				//We must first and foremost connect to Photon Online Server.
				//Users are separated from each other by game version (which allows you to make breaking changes).
				//In PhotonServerSettings, Hosting is set to Best Region excluding South Korea, Asia and Japan
				if( playerDebugConfiguration.getOverrideCloudRegionCode() != CloudRegionCode.none ) PhotonNetwork.OverrideBestCloudServer( playerDebugConfiguration.getOverrideCloudRegionCode() );
				PhotonNetwork.ConnectUsingSettings(GameManager.Instance.getVersionNumber());
				Debug.Log("GameManager-PhotonNetwork.versionPUN is " + PhotonNetwork.versionPUN );
			}
		}
	} 

	public PlayMode getPlayMode()
	{
		return playMode;
	}

	/// <summary>
	/// Returns true for the following play modes: PlayAgainstOnePlayer, PlayAgainstTwoPlayers, PlayAgainstOneFriend, and PlayCoopWithOnePlayer.
	/// </summary>
	/// <returns><c>true</c>, for the following play modes: PlayAgainstOnePlayer, PlayAgainstTwoPlayers, PlayAgainstOneFriend, and PlayCoopWithOnePlayer</returns>
	public bool isOnlinePlayMode()
	{
		return playMode == PlayMode.PlayAgainstOnePlayer || playMode == PlayMode.PlayAgainstTwoPlayers || playMode == PlayMode.PlayAgainstOneFriend || playMode == PlayMode.PlayCoopWithOnePlayer;
	}

	public bool isCoopPlayMode()
	{
		return playMode == PlayMode.PlayCoopWithOneBot || playMode == PlayMode.PlayCoopWithOnePlayer;
	}

	//Note the version number stored in version.txt should match the bundle version in PlayerSettings.
	public void loadVersionNumber()
	{
		TextAsset versionFile =  Resources.Load("Version/version") as TextAsset;
		if( versionFile != null )
		{
			versionNumber = versionFile.text;
		}
		else
		{
			Debug.LogError("GameManager-loadVersionNumber error: unable to find Version/version.txt");
		}
	}

	//Returns the version number, for example v1.12.3
	//The version number is displayed in the settings menu
	public string getVersionNumber()
	{
		return versionNumber;
	}

	public GameMode getGameMode()
	{
		return gameMode;
	}

	public void setGameMode( GameMode value )
	{
		gameMode = value;
	}

	public void setMultiplayerMode( bool value )
	{
		multiplayerMode = value;
	}

	public bool isMultiplayer()
	{
		return multiplayerMode;
	}

	//The global coin multiplier is used to increase (value bigger than 1) or decrease (value smaller than 1) the
	//percentage chance that coin packs appear in the level.
	//Also see CoinHandler.
	public float getGlobalCoinMultiplier()
	{
		return 1f;
	}

	//The global stumble multiplier is used to increase (value bigger than 1) or decrease (value smaller than 1) the
	//percentage chance that a stumble obstacle will appear in the level.
	public float getGlobalStumbleMultiplier()
	{
		return 1f;
	}

}
