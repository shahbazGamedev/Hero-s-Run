using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This class is a Singleton
public class LevelManager {

	private static LevelManager levelManager = null;
	private LevelData levelData = null;
	private int nextLevelToComplete = 0;
	private LevelData.LevelInfo currentLevelInfo = null;
	private bool levelHasChanged = false;
	private bool playerFinishedTheGame = false;
	private int levelNumberOflastCheckpoint = 0; //Either the first level of the current theme OR the level of the last checkpoint
 	private int score = 0; //currently is equal to the number of stars you picked up while running for a single episode
	private int currentEpisode = 0;
	private bool episodeCompleted = false;

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
			Debug.Log ("LevelManager-setNextLevelToComplete: " + nextLevelToComplete );
		}
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

	//For debugging
	//Called by WorldMapHandler to access any level directly
	public void forceNextLevelToComplete( int levelToComplete )
	{
		nextLevelToComplete = levelToComplete;
		Debug.Log ("LevelManager-setNextLevelToComplete: " + nextLevelToComplete );
	}

	public int getNextLevelToComplete()
	{
		return nextLevelToComplete;
	}

	public bool isTutorialActive()
	{
		if( currentLevelInfo != null )
		{
			return currentLevelInfo.isTutorial;
		}
		else
		{
			return false;
		}
	}

	public void unlockAllLevels()
	{
		nextLevelToComplete = levelData.levelList.Count -1;
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
			//Update our Facebook score
			//The Facebook score is simply the highest level reached so far in the game.
			FacebookManager.Instance.postHighScore( newLevel );
			Debug.Log("LevelManager-incrementNextLevelToComplete : nextLevelToComplete: " + nextLevelToComplete + " " + levelHasChanged );
			return false;

		}
		else
		{
			Debug.Log("LevelManager-incrementNextLevelToComplete : you have finished the game. Congratulations." );
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
		Debug.LogWarning( "setCurrentEpisodeNumber " + currentEpisode );
    }

	public void incrementCurrentEpisodeNumber()
	{
		if( currentEpisode < ( LevelData.NUMBER_OF_EPISODES - 1 ) )
		{
			currentEpisode++;
			Debug.LogWarning( "incrementCurrentEpisodeNumber " + currentEpisode );
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
	
	//Called by GenerateLevel on Awake()
	//Sets the level data for the entire level.
	public void setLevelData( LevelData levelData )
	{
		this.levelData = levelData;
	}

	public LevelData getLevelData()
	{
		return levelData;
	}

	//Returns true if locked, false otherwise.
	public bool isLevelLocked( int levelNumber )
	{
		return levelData.getLevelInfo( levelNumber ).isLevelLocked;
	}

	public string getLevelDescription( int levelNumber )
	{
		string levelDescriptionTextID = levelData.getLevelInfo( levelNumber ).LevelDescription;
		return LocalizationManager.Instance.getText( levelDescriptionTextID );
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

}
