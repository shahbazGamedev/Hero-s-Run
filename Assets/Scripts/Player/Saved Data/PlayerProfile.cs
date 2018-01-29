using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerProfileEvent {
	Level_Changed = 0,
	Player_Icon_Changed = 1,
	XP_Changed = 2,
	User_Name_Changed = 3,
	Trophies_Changed = 4
}

public enum SectorStatus {
	
	NO_CHANGE = 0,
	WENT_UP = 1,
	WENT_DOWN = -1,
	WENT_UP_AND_NEW = 2
}

[System.Serializable]
public sealed class PlayerProfile {

	[SerializeField] string userName;
	public int totalXPEarned = 0;
	public int xpProgressToNextLevel = 0;
	public int level = 1;
	public string lastMatchPlayedTimeString = "01/01/1970 00:00:00";
	public string lastMatchWonTimeString = "01/01/1970 00:00:00";
	private DateTime lastMatchPlayedTime = new DateTime(1970,01,01);
	private DateTime lastMatchWonTime = new DateTime(1970,01,01);

	[Header("Rate this App")]
	public string lastTimeRateThisAppWasShownString = "01/01/1970 00:00:00";
	private DateTime lastTimeRateThisAppWasShown = new DateTime(1970,01,01);
	public bool didPlayerRateApp = false;
	public int timesRateThisAppDisplayed = 0;
	[System.NonSerialized] int consecutiveRacesWon = 0;

	[Header("Other")]
	//This is the unique Id of the player icon.
	//By default, the Id is zero. This is the Id of the default player icon that new players have.
	//The user can change his player icon in the Player Icon screen.
	[SerializeField] int playerIconId = 0;
	public int selectedHeroIndex; //index for heroCharacterList in HeroManager

	[SerializeField] bool completedTutorial = false;
	//Trophies indicate your success in racing. Players gain or lose Trophies by either winning or losing races in online multiplayer races.
	//The number of trophies you have indicate which race track you will be racing in for multiplayer races.
	[SerializeField] int numberOfTrophies = 0;
 	//Not serialized. trophiesEarnedOrLost is set by PlayerRaceManager in the Level scene, but must be read by GameEndManager in the Matchmaking Scene.
	private int trophiesEarnedLastRace = 0;

	#region Sector 
	[SerializeField] int currentSector = 0; 	//Current sector. A new player starts off in sector 0. Between 0 and SectorManager.MAX_SECTOR
	[SerializeField] int highestSector = 0; 	//Highest sector unlocked. Between 0 and SectorManager.MAX_SECTOR
	public delegate void SectorChanged( SectorStatus sectorChanged, int previousSector, int newSector );
	public static event SectorChanged sectorChanged;
	#endregion

	//Skill points are obtained during a race and awarded as an XP bonus when the race is completed.
	//Therefore there is no need to serialize since they get added to the player's XP total.
	[System.NonSerialized] int skillBonus = 0;

	//Delegate used to communicate to other classes when the a value changes
	public delegate void PlayerProfileChanged( PlayerProfileEvent eventType, int previousValue = 0, int newValue = 0 );
	public static event PlayerProfileChanged playerProfileChanged;


	#region User Name
	public string getUserName()
	{
		return userName;
	}
	
	/// <summary>
	/// Saves the user name and sends a PlayerProfileEvent.User_Name_Changed.
	/// </summary>
	/// <param name="value">Value.</param>
	public void saveUserName( string value )
	{
		userName = value;
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.User_Name_Changed );
		serializePlayerprofile();
	}
	#endregion

	#region Tutorial
	public void setCompletedTutorial( bool value)
	{
		completedTutorial = value;
		Debug.Log("PlayerProfile-setCompletedTutorial: " + value );
		serializePlayerprofile();
	}

	public bool hasCompletedTutorial()
	{
		return completedTutorial;
	}
	#endregion

	#region Skill Bonus
	public int getSkillBonus()
	{
		return skillBonus;
	}

	public void addToSkillBonus( int value )
	{
		skillBonus = skillBonus + value;
	}

	public void resetSkillBonus()
	{
		skillBonus = 0;
	}
	#endregion

	#region Trophies
	public int getTrophiesEarnedLastRace()
	{
		return trophiesEarnedLastRace;
	}

	public void setTrophiesEarnedLastRace( int value )
	{
		trophiesEarnedLastRace = value;
	}

	public int getTrophies()
	{
		return numberOfTrophies;
	}

	/// <summary>
	/// Changes the trophies.
	/// Don't allow the number of trophies to go below 1. If a player loses while in sector 1, we don't want him to go down to sector 0, which is the tutorial.
	/// </summary>
	/// <param name="value">Value.</param>
	public void changeTrophies( int value )
	{
		if( value >= -TrophyManager.MAX_CHANGE_IN_TROPHIES && value <= TrophyManager.MAX_CHANGE_IN_TROPHIES )
		{
			int newValue = numberOfTrophies + value;
			if( newValue  < 1 ) newValue = 1;
			setNumberOfTrophies( newValue );
		}
		else
		{
			Debug.LogWarning("PlayerProfile-the number of trophies to change " + value + " is incorrect. It needs to be between " + (-TrophyManager.MAX_CHANGE_IN_TROPHIES).ToString() + " and " + TrophyManager.MAX_CHANGE_IN_TROPHIES.ToString() + ".");
		}
	}

	public void setNumberOfTrophies( int value )
	{
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Trophies_Changed, numberOfTrophies, value );

		numberOfTrophies = value;
		if( numberOfTrophies == 0 )
		{
			setCompletedTutorial( false );
		}
		else
		{
			setCompletedTutorial( true );
		}
		GameManager.Instance.playerStatistics.setHighestNumberOfTrophies( numberOfTrophies );
		//Verify if the player changed sector because of the change in trophies.
		verifyIfSectorChanged();
		Debug.Log("PlayerProfile-setNumberOfTrophies to: " + value );
	}

	public void verifyIfSectorChanged()
	{
		int sectorAfterTrophyChange = SectorManager.Instance.getSectorByTrophies( numberOfTrophies );

		if( sectorAfterTrophyChange > currentSector )
		{
			if( sectorAfterTrophyChange > highestSector )
			{
				if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_UP_AND_NEW, currentSector, sectorAfterTrophyChange );
			}
			else
			{
				if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_UP, currentSector, sectorAfterTrophyChange );
			}
		}
		else if( sectorAfterTrophyChange < currentSector )
		{
			if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_DOWN, currentSector, sectorAfterTrophyChange );
		}
		setCurrentSector( sectorAfterTrophyChange );
	}
	#endregion

	#region Sector
	public int getCurrentSector()
	{
		return currentSector;
	}

	void setCurrentSector( int value )
	{
		//Ignore if the sector has not changed.
		if( value == currentSector ) return;

		if( value >= 0 && value <= SectorManager.MAX_SECTOR )
		{
			if( value > currentSector ) highestSector = value;
			currentSector = value;
			serializePlayerprofile();
			Debug.Log("PlayerProfile-setting current sector to: " + value );
		}
		else
		{
			Debug.LogError("PlayerProfile-the sector specified " + value + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
		}
	}

	public int getHighestSector()
	{
		return highestSector;
	}
	#endregion

	public int getLevel()
	{
		return level;
	}

	public void incrementLevel()
	{
		setLevel( level + 1 );
	}

	void setLevel( int value )
	{
		if( value > 0 && value <= ProgressionManager.MAX_LEVEL )
		{
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Level_Changed, level, value );
			level = value;
			Debug.Log("PlayerProfile-setting level to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerProfile-the level specified " + value + " is incorrect. It needs to be between 1 and " + ProgressionManager.MAX_LEVEL.ToString() + ".");
		}
	}

	public void addToTotalXPEarned( int xpAmount, bool saveImmediately )
	{
		if( xpAmount <= 0 || xpAmount > ProgressionManager.MAX_XP_IN_ONE_RACE ) return;
		int previousAmount = totalXPEarned;
		totalXPEarned = totalXPEarned + xpAmount;
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.XP_Changed, previousAmount, totalXPEarned );
		if( saveImmediately ) serializePlayerprofile();
		Debug.Log("PlayerProfile-addXP: adding XP: " + xpAmount + " New total is: " +  totalXPEarned );
	}

	public void setLastMatchPlayedTime( DateTime timeMatchPlayed )
	{
		lastMatchPlayedTime = timeMatchPlayed;
		lastMatchPlayedTimeString = lastMatchPlayedTime.ToString();
	}

	public DateTime getLastMatchPlayedTime()
	{
		lastMatchPlayedTime = Convert.ToDateTime (lastMatchPlayedTimeString); 
		return lastMatchPlayedTime; 
	}

	public void setLastMatchWonTime( DateTime timeMatchWon )
	{
		lastMatchWonTime = timeMatchWon;
		lastMatchWonTimeString = lastMatchWonTime.ToString();
	}

	public DateTime getLastMatchWonTime()
	{
		lastMatchWonTime = Convert.ToDateTime (lastMatchWonTimeString); 
		return lastMatchWonTime.Date; //We don't want the time element, only the date
	}

	#region Rate this App
	public void setLastTimeRateThisAppWasShown( DateTime lastTimeRateThisAppWasShown )
	{
		this.lastTimeRateThisAppWasShown = lastTimeRateThisAppWasShown;
		lastTimeRateThisAppWasShownString = lastTimeRateThisAppWasShown.ToString();
		serializePlayerprofile();
	}

	public DateTime getLastTimeRateThisAppWasShown()
	{
		lastTimeRateThisAppWasShown = Convert.ToDateTime (lastTimeRateThisAppWasShownString); 
		return lastTimeRateThisAppWasShown;
	}

	public void resetConsecutiveWins()
	{
		consecutiveRacesWon = 0;
	}

	public void incrementConsecutiveWins()
	{
		consecutiveRacesWon++;
	}

	public int getConsecutiveWins()
	{
		return consecutiveRacesWon;
	}
	#endregion

	#region Player Icon
	public void setPlayerIconId( int value )
	{
		if( value >= 0 && value < ProgressionManager.Instance.getNumberOfPlayerIcons() )
		{
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Player_Icon_Changed, playerIconId, value );
			playerIconId = value;
		}
		else
		{
			Debug.LogWarning("PlayerProfile-setPlayerIconId: the value specified " + value + " is incorrect. It needs to be between 0 and " + ProgressionManager.Instance.getNumberOfPlayerIcons() + " exclusively." );
		}
	}

	public int getPlayerIconId()
	{
		return playerIconId;
	}
	#endregion

	public void serializePlayerprofile()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerProfile( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}
}
