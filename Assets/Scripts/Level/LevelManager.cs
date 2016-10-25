using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Level_Progress {
	
	LEVEL_START = 0,
	LEVEL_END_WITH_NO_PROGRESS = 1,
	LEVEL_END_WITH_PROGRESS = 2,
	LEVEL_END_WITH_GAME_COMPLETED = 3
}

//This class is a Singleton
public class LevelManager {

	private static LevelManager levelManager = null;
	private LevelData levelData = null;
	private int nextLevelToComplete = 0;
	private int highestLevelCompleted = 0;
	private LevelData.LevelInfo currentLevelInfo = null;
	private bool levelHasChanged = false;
	private bool playerFinishedTheGame = false;
	private int levelNumberOflastCheckpoint = 0; //Either the first level of the current theme OR the level of the last checkpoint
 	private int score = 0; //currently is equal to the number of stars you picked up while running for a single episode
	private int currentEpisode = 0;
	private bool episodeCompleted = false;
	private bool enableTorches = true;

	public static LevelManager Instance
	{
        get
		{
            if (levelManager == null)
			{

                levelManager = new LevelManager();

            }
            return levelManager;
        }
    } 

	//Called by PlayerStatsManager
	public void setNextLevelToComplete( int levelToComplete )
	{
		if( levelToComplete > nextLevelToComplete )
		{
			nextLevelToComplete = levelToComplete;
			Debug.Log ("LevelManager-setNextLevelToComplete: nextLevelToComplete " + nextLevelToComplete );
			
		}
	}

	public int getNextLevelToComplete()
	{
		return nextLevelToComplete;
	}

	public void setLevelNumberOfLastCheckpoint( int previousCheckpoint )
	{
		levelNumberOflastCheckpoint = previousCheckpoint;
		Debug.Log ("LevelManager-setLevelNumberOflastCheckpoint: " + levelNumberOflastCheckpoint );
	}

	public int getLevelNumberOfLastCheckpoint()
	{
		Debug.Log ("LevelManager-getLevelNumberOfLastCheckpoint: " + levelNumberOflastCheckpoint );
		return levelNumberOflastCheckpoint;
	}

	//Called by EpisodePopup to access any level directly
	public void forceNextLevelToComplete( int levelToComplete )
	{
		nextLevelToComplete = levelToComplete;
		Debug.Log ("LevelManager-forceNextLevelToComplete: " + nextLevelToComplete );
	}
	
	public void setHighestLevelCompleted( int levelCompleted )
	{
		if( levelCompleted > highestLevelCompleted )
		{
			highestLevelCompleted = levelCompleted;
			Debug.Log ("LevelManager-setHighestLevelCompleted: highestLevelCompleted " + highestLevelCompleted );
			
		}
	}

	public int getHighestLevelCompleted()
	{
		return highestLevelCompleted;
	}

	public void unlockAllLevels()
	{
		highestLevelCompleted = levelData.levelList.Count -1;
	}

	//Called when the cullis gate activation is complete or 
	//when a level checkpoint is triggered.
	//Returns true if the final level has been completed (and therefore the game is finished) and false otherwise.
	public bool incrementNextLevelToComplete()
	{
		int newLevel = nextLevelToComplete + 1;
		if( newLevel < levelData.levelList.Count )
		{
			nextLevelToComplete = newLevel;
			setLevelChanged( true );
			setHighestLevelCompleted( newLevel );
			Debug.Log("LevelManager-incrementNextLevelToComplete : nextLevelToComplete: " + nextLevelToComplete + " " + levelHasChanged );
			return false;

		}
		else
		{
			Debug.Log("LevelManager-incrementNextLevelToComplete : you have finished the game. Congratulations." );
			setHighestLevelCompleted( levelData.levelList.Count - 1 );
			setPlayerFinishedTheGame( true );
			return true;
		}
	}

	public int getCurrentEpisodeNumber()
	{
		return currentEpisode;
    }

	public void setCurrentEpisodeNumber( int currentEpisode )
	{
		this.currentEpisode = currentEpisode;
		Debug.Log( "setCurrentEpisodeNumber " + currentEpisode );
    }

	public void incrementCurrentEpisodeNumber()
	{
		if( currentEpisode < ( LevelData.NUMBER_OF_EPISODES - 1 ) )
		{
			currentEpisode++;
			Debug.Log( "incrementCurrentEpisodeNumber " + currentEpisode );
		}		
    }

	//Used by Post-level popup
	public bool wasEpisodeCompleted()
	{
		return episodeCompleted;
    }

	//Set by CullisGateController
	public void setEpisodeCompleted( bool episodeCompleted )
	{
		this.episodeCompleted = episodeCompleted;
	}

	public int getScore()
    {
		return score;
	}
	
	public void setScore( int value )
	{
		score = value;
    }

	public void incrementScore( int scoreToAdd )
	{
		score = score + scoreToAdd;
    }

	public bool getPlayerFinishedTheGame()
	{
		return playerFinishedTheGame;
	}

	public void setPlayerFinishedTheGame( bool didCompleteGame )
	{
		playerFinishedTheGame = didCompleteGame;
		Debug.Log ("LevelManager-setPlayerFinishedTheGame: " + playerFinishedTheGame );
	}

	public int getNumberOfLevels()
	{
		return levelData.levelList.Count;
	}
	
	//Called by TitleScreenHandler on Awake()
	public void setLevelData( LevelData levelData )
	{
		this.levelData = levelData;
		currentEpisode = getEpisodeNumberFromLevelNumber( nextLevelToComplete );
		Debug.Log ("LevelManager-setLevelData: currentEpisode " + currentEpisode );
	}

	public int getEpisodeNumberFromLevelNumber( int levelNumber )
	{
		//Figure out which episode this level corresponds to
		int episodeCounter = -1;
		List<LevelData.LevelInfo> levelList = levelData.getLevelList();		
		for( int i = 0; i < levelList.Count; i++ )
		{
			if(levelList[i].levelType == LevelType.Episode ) episodeCounter++;
			if( i == levelNumber ) break;
		}
		//Debug.Log ("LevelManager-getEpisodeNumberFromLevelNumber: levelNumber " + levelNumber + " " + episodeCounter );
		return episodeCounter;
	}

	public int getLevelNumberFromEpisodeNumber( int episodeNumber )
	{
		//Figure out which level this corresponds to
		int episodeCounter = -1;
		int levelCounter = -1;
		List<LevelData.LevelInfo> levelList = levelData.getLevelList();		
		foreach( LevelData.LevelInfo aLevel in levelList )
		{
			if( aLevel.levelType == LevelType.Episode ) episodeCounter++;
			levelCounter++;
			if( episodeCounter >= episodeNumber ) break;
		}
		//Debug.Log ("LevelManager-getLevelNumberFromEpisodeNumber : " + episodeNumber  + " corresponds to level " + levelCounter );
		return levelCounter;
	}

	public LevelData getLevelData()
	{
		return levelData;
	}

	public void setLevelChanged( bool hasChanged )
	{
		levelHasChanged = hasChanged;
		Debug.Log("LevelManager-setLevelChanged: " + levelHasChanged );
	}

	public bool getLevelChanged()
	{
		return levelHasChanged;
	}
	
	//Called by GenerateLevel on Start()
	//Sets the level info for the current level.
	public void setLevelInfo( LevelData.LevelInfo levelInfo )
	{
		currentLevelInfo = levelInfo;
	}
		
	public LevelData.LevelInfo getLevelInfo()
	{
		return currentLevelInfo;
	}
	
	public LevelData.LevelInfo getLevelInfo( int levelNumber )
	{
		return levelData.levelList[levelNumber];
	}

	public LevelData.EpisodeInfo getCurrentEpisodeInfo()
	{
		return levelData.episodeList[currentEpisode];
	}

	public string getCurrentLevelName()
	{
		return LocalizationManager.Instance.getText( currentLevelInfo.LevelName );
	}

	public string getNextLevelName()
	{
		int nextLevel = nextLevelToComplete + 1;
		if( nextLevel < levelData.levelList.Count )
		{
			string levelName = LocalizationManager.Instance.getText( levelData.levelList[nextLevel].LevelName );
			if( levelName == "NOT FOUND" ) 
			{
				return levelData.levelList[nextLevel].LevelName;
			}
			else
			{
				return levelName;
			}
		}
		else
		{
			string levelName = LocalizationManager.Instance.getText( levelData.FinalDestinationName );
			if( levelName == "NOT FOUND" ) 
			{
				return levelData.FinalDestinationName;
			}
			else
			{
				return levelName;
			}
		}
	}

	//See also TorchHandler.
	public void setEnableTorches( bool value )
	{
		enableTorches = value;
	}

	public bool getEnableTorches()
	{
		return enableTorches;
	}

}
