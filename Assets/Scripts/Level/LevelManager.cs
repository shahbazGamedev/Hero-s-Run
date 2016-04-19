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

	public int getNextSectionToUnlock()
	{
		int startSection = PlayerStatsManager.Instance.getHighestLevelUnlocked() + 1; //Exclude the current section, hence the plus one.
		for( int i=startSection; i < levelData.levelList.Count; i++ )
		{
			if( levelData.levelList[i].isSectionEnd )
			{
				return i;
			}
		}
		//No section end was found. In that case, return the last level.
		return levelData.levelList.Count-1;
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
			levelHasChanged = true;
			//Update our Facebook score
			//The Facebook score is simply the highest level reached so far in the game.
			FacebookManager.Instance.postHighScore( newLevel );
			Debug.Log("LevelManager-incrementNextLevelToComplete : nextLevelToComplete: " + nextLevelToComplete + " " + levelHasChanged );
			return false;

		}
		else
		{
			Debug.Log("LevelManager-incrementNextLevelToComplete : you have finished the game. Congratulations." );
			playerFinishedTheGame = true;
			return true;
		}
	}

	public bool getPlayerFinishedTheGame()
	{
		return playerFinishedTheGame;
	}

	public void setPlayerFinishedTheGame( bool didCompleteGame )
	{
		playerFinishedTheGame = didCompleteGame;
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

	public string getSectionName( int levelNumber )
	{
		return levelData.getLevelInfo( levelNumber ).sectionName;
	}

	public string getLevelDescription( int levelNumber )
	{
		string levelDescriptionTextID = levelData.getLevelInfo( levelNumber ).LevelDescription;
		return LocalizationManager.Instance.getText( levelDescriptionTextID );
	}

	public void setLevelChanged( bool hasChanged )
	{
		levelHasChanged = hasChanged;
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
	
	public string getCurrentLevelName()
	{
		string levelName = LocalizationManager.Instance.getText( currentLevelInfo.LevelName );
		if( levelName == "NOT FOUND" ) 
		{
			return currentLevelInfo.LevelName;
		}
		else
		{
			return levelName;
		}
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
