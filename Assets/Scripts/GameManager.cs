using UnityEngine;
using System.Collections;


public enum GameState {
	
	Unknown = 0,
	Normal = 1,
	Paused = 2,
	Countdown = 3,
	SaveMe = 4,
	Resurrect = 5,
	StatsScreen = 6,
	Menu = 7,
	Checkpoint = 8,
	BeforeTapToPlayAllowed = 9,
	PostLevelPopup = 10
}

//The GameScenes enum entries must match the scene numbering in Build Settings.
//Sample usage: SceneManager.LoadScene( (int)GameScenes.CharacterSelection )
public enum GameScenes {	
	
	TitleScreen = 0,
	CharacterSelection = 1,
	WorldMap = 2,
	Level = 3,
	TreasureIsland = 4,
	CharacterGallery = 5,
	Store = 6
}

public enum DifficultyLevel {
	
	Normal = 1,
	Heroic = 2,
	Legendary = 3
}

public enum GameMode {
	
	Story = 1,
	Endless = 2
}


//This class is a Singleton
public class GameManager {

	private GameState gameState = GameState.Unknown;
	private string versionNumber = System.String.Empty;

	private static GameManager gameManager = null;

	//Event management used to notify other classes when the game state has changed
	public delegate void GameStateEvent( GameState value );
	public static event GameStateEvent gameStateEvent;

	private GameMode gameMode = GameMode.Story;
	private int gameClock = 465; //In elapsed minutes. This is related to episode.

	public static GameManager Instance
	{
        get
		{
            if (gameManager == null)
			{

                gameManager = new GameManager();
				if( Debug.isDebugBuild )
				{
					Debug.Log("GameManager: the quality setting for this device is: " + QualitySettings.GetQualityLevel() );
				}

            }
            return gameManager;
        }
    } 
	
	public void setGameState( GameState state )
	{
		gameState = state;
		Debug.Log("setGameState: new state is " + state );
		//Send an event to interested classes
		if(gameStateEvent != null) gameStateEvent( gameState );

	}

	public GameState getGameState()
	{
		return gameState;
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

	public int getGameClock()
	{
		return gameClock;
	}

	public void setGameClock( int value )
	{
		gameClock = value;
	}

}
