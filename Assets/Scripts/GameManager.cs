using UnityEngine;
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
	WorldMap = 1,
	Level = 2,
	TreasureIsland = 3,
	CharacterGallery = 4,
	Journal = 5,
	CircuitSelection = 6,
	HeroSelection = 7,
	Matchmaking = 8,
	CareerProfile = 9,
	MainMenu = 10,
	PlayModes = 11,
	Options = 12,
	Training = 13,
	Social = 14
}

public enum GameMode {
	
	Story = 1,
	Endless = 2
}

public enum PlayMode {
	
	PlayTwoPlayers = 1,
	PlayWithFriends = 2,
	PlayAgainstEnemy = 3,
	PlayAlone = 4,
	PlayThreePlayers = 5

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
	PlayMode playMode = PlayMode.PlayTwoPlayers;

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
			case PlayMode.PlayAgainstEnemy:
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
			break;

			case PlayMode.PlayAlone:
				LevelManager.Instance.setNumberOfPlayersRequired( 1 );
			break;

			case PlayMode.PlayTwoPlayers:
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
			break;

			case PlayMode.PlayThreePlayers:
				LevelManager.Instance.setNumberOfPlayersRequired( 3 );
			break;

			case PlayMode.PlayWithFriends:
				LevelManager.Instance.setNumberOfPlayersRequired( 2 );
			break;
		}
	} 

	public PlayMode getPlayMode()
	{
		return playMode;
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
