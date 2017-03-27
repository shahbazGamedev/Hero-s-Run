﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Level_Progress {
	
	EPISODE_START = 0,
	EPISODE_END_WITH_NO_PROGRESS = 1,
	EPISODE_END_WITH_PROGRESS = 2,
	EPISODE_END_WITH_GAME_COMPLETED = 3
}

//This class is a Singleton
public class LevelManager {

	private static LevelManager levelManager = null;
	private LevelData levelData = null;
	private int nextEpisodeToComplete = 0;
	private int highestEpisodeCompleted = 0;
	private bool episodeHasChanged = false;
	private bool playerFinishedTheGame = false;
	private int numberOfCheckpointsPassed = 0; //Number of checkpoints passed for the current episode
	//Number of coins the player had when he passed the last checkpoint.
	//If the player dies and restarts at a checkpoint, this will be the number of coins he will have.
	private int numberOfCoinsAtLastCheckpoint = 0;
 	private int score = 0; //currently is equal to the number of coins you picked up while running for a single episode
	private int currentEpisode = 0;
	private bool episodeCompleted = false;
	private bool enableTorches = true;
	//onlyUseUniqueTiles is only used for testing. This value is not saved. When enabled, only tile groups with a Frequency of Unique will appear
	//in the level while in Story mode. This allows you to have shorter episodes with only the essential tile groups, typically, Start, any scripted sequence, and End.
	bool onlyUseUniqueTiles = false;
	byte numberOfPlayersRequired;
	private int currentMultiplayerLevel = 0; //the index of the multiplayer level selected by the player.
	public int selectedBotHeroIndex; //index for botHeroCharacterList in HeroManager. Used when in the PlayAgainstEnemy mode.
	public ChatManager.MatchInvitation matchInvitation;
	public float distanceTravelled = 0; //See PlayerRace. This value is displayed in the HUD.

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
	public void setNextEpisodeToComplete( int episodeToComplete )
	{
		if( episodeToComplete > nextEpisodeToComplete )
		{
			nextEpisodeToComplete = episodeToComplete;
		}
	}

	public int getNextEpisodeToComplete()
	{
		return nextEpisodeToComplete;
	}

	public void incrementNumberOfCheckpointsPassed()
	{
		numberOfCheckpointsPassed++;
		Debug.Log ("LevelManager-incrementNumberOfCheckpointsPassed: " + numberOfCheckpointsPassed );
	}

	public int getNumberOfCheckpointsPassed()
	{
		Debug.Log ("LevelManager-getNumberOfCheckpointsPassed: " + numberOfCheckpointsPassed );
		return numberOfCheckpointsPassed;
	}

	public void resetNumberOfCheckpointsPassed()
	{
		Debug.Log ("LevelManager-resetNumberOfCheckpointsPassed" );
		numberOfCheckpointsPassed = 0;
	}

	public void setCoinsAtLastCheckpoint( int value )
	{
		numberOfCoinsAtLastCheckpoint = value;
		Debug.Log ("LevelManager-setCoinsAtLastCheckpoint: " + numberOfCoinsAtLastCheckpoint );
	}

	public int getCoinsAtLastCheckpoint()
	{
		Debug.Log ("LevelManager-getCoinsAtLastCheckpoint: " + numberOfCoinsAtLastCheckpoint );
		return numberOfCoinsAtLastCheckpoint;
	}

	public void resetCoinsAtLastCheckpoint()
	{
		Debug.Log ("LevelManager-resetCoinsAtLastCheckpoint" );
		numberOfCoinsAtLastCheckpoint = 0;
	}

	//Called by EpisodePopup to access any episode directly
	public void forceNextEpisodeToComplete( int episodeToComplete )
	{
		nextEpisodeToComplete = episodeToComplete;
		Debug.Log ("LevelManager-forceNextEpisodeToComplete: " + nextEpisodeToComplete );
	}
	
	public void setHighestEpisodeCompleted( int episodeCompleted )
	{
		if( episodeCompleted > highestEpisodeCompleted )
		{
			highestEpisodeCompleted = episodeCompleted;
		}
	}

	public int getHighestEpisodeCompleted()
	{
		return highestEpisodeCompleted;
	}

	public void unlockAllEpisodes()
	{
		highestEpisodeCompleted = levelData.episodeList.Count -1;
	}

	//Called when the cullis gate activation is complete.
	//Returns true if the final level has been completed (and therefore the game is finished) and false otherwise.
	public bool incrementNextEpisodeToComplete()
	{
		int newEpisode = nextEpisodeToComplete + 1;
		if( newEpisode < levelData.episodeList.Count )
		{
			nextEpisodeToComplete = newEpisode;
			setEpisodeChanged( true );
			setHighestEpisodeCompleted( newEpisode );
			Debug.Log("LevelManager-incrementNextEpisodeToComplete : nextEpisodeToComplete: " + nextEpisodeToComplete + " " + episodeHasChanged );
			return false;

		}
		else
		{
			Debug.Log("LevelManager-incrementNextEpisodeToComplete : you have finished the game. Congratulations." );
			setHighestEpisodeCompleted( levelData.episodeList.Count - 1 );
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
	}

	//Called by TitleScreenHandler on Awake()
	public void setLevelData( LevelData levelData )
	{
		this.levelData = levelData;
	}

	public LevelData getLevelData()
	{
		return levelData;
	}

	public void setEpisodeChanged( bool hasChanged )
	{
		episodeHasChanged = hasChanged;
		Debug.Log("LevelManager-setEpisodeChanged: " + episodeHasChanged );
	}

	public bool getEpisodeChanged()
	{
		return episodeHasChanged;
	}
	
	public LevelData.EpisodeInfo getCurrentEpisodeInfo()
	{
		return levelData.episodeList[currentEpisode];
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

	public void setOnlyUseUniqueTiles( bool value )
	{
		onlyUseUniqueTiles = value;
	}

	public bool getOnlyUseUniqueTiles()
	{
		return onlyUseUniqueTiles;
	}

	public void setNumberOfPlayersRequired( byte value )
	{
		numberOfPlayersRequired = value;
	}

	public byte getNumberOfPlayersRequired()
	{
		return numberOfPlayersRequired;
	}

	public int getCurrentMultiplayerLevel()
	{
		return currentMultiplayerLevel;
    }

	public void setCurrentMultiplayerLevel( int selectedLevel )
	{
		if( selectedLevel != currentMultiplayerLevel )
		{
			currentMultiplayerLevel = selectedLevel;
			Debug.Log( "setCurrentMultiplayerLevel " + selectedLevel );
		}
    }

	public int getNumberOfMultiplayerLevels()
	{
		return levelData.multiplayerList.Count;
    }

	public LevelData.MultiplayerInfo getSelectedMultiplayerLevel()
	{
		return levelData.multiplayerList[currentMultiplayerLevel];
    }

	public LevelData.CircuitInfo getSelectedCircuitInfo()
	{
		return levelData.multiplayerList[currentMultiplayerLevel].circuitInfo;
    }

}
