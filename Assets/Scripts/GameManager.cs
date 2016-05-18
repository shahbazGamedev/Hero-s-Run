using UnityEngine;
using System.Collections;
using System;


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
	PostLevelPopup = 9
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
	//The game starts at dawn, specifically at 6:30AM
	private TimeSpan timeOfDay = new TimeSpan( 6, 30, 0 );
	public delegate void TimeOfDayEvent( TimeSpan value );
	public static event TimeOfDayEvent timeOfDayEvent;

	public const int TIME_PENALTY_IN_MINUTES = 5;

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

	public TimeSpan calculateTimeOfDay()
	{
		//Get time of day for current level
		Vector2 levelTimeOfDay = LevelManager.Instance.getLevelInfo(LevelManager.Instance.getNextLevelToComplete()).timeOfDay;
		//Calculate time penalty
		int penaltyInMinutes = PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) * TIME_PENALTY_IN_MINUTES;
		timeOfDay = new TimeSpan((int)levelTimeOfDay.x, (int)levelTimeOfDay.y, 0 );
		TimeSpan span = TimeSpan.FromMinutes(penaltyInMinutes);
		timeOfDay = timeOfDay.Add(span);
		//Send an event to interested classes
		if(timeOfDayEvent != null) timeOfDayEvent( timeOfDay );
		Debug.Log( "calculateTimeOfDay " + penaltyInMinutes + " " + PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) + " H: " + timeOfDay.Hours + " M: " + + timeOfDay.Minutes + " Episode: " + LevelManager.Instance.getCurrentEpisodeNumber() );
		return timeOfDay;
	}

	public TimeSpan getTimeOfDay()
	{
		return timeOfDay;
	}

	public void setTimeOfDay( int additionalMinutes )
	{
		TimeSpan span = TimeSpan.FromMinutes(additionalMinutes);
		timeOfDay = timeOfDay.Add(span);
		//Send an event to interested classes
		if(timeOfDayEvent != null) timeOfDayEvent( timeOfDay );
	}

}
