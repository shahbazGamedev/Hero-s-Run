using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerProfileEvent {
	Player_Icon_Changed = 1,
	XP_Changed = 2,
	User_Name_Changed = 3,
	Competitive_Points_Changed = 4
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

	[SerializeField] int totalXPEarned = 0;
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
	//Competitive points (CP) indicate your success in racing. Players gain or lose CP by either winning or losing races in online multiplayer races.
	[SerializeField] int competitivePoints = 0;

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
		serializePlayerprofile( true );
	}
	#endregion

	#region Tutorial
	public void setCompletedTutorial( bool value)
	{
		completedTutorial = value;
		Debug.Log("PlayerProfile-setCompletedTutorial: " + value );
		serializePlayerprofile( true );
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

	#region Competitive Points (also known as CP)
	public int getCompetitivePoints()
	{
		return competitivePoints;
	}

	/// <summary>
	/// Changes the competitive points.
	/// Don't allow the number of CP to go below 1. If a player loses while in sector 1, we don't want him to go down to sector 0, which is the tutorial.
	/// </summary>
	/// <param name="value">Value.</param>
	public void changeCompetitivePoints( int value )
	{
		if( value >= -CompetitionManager.MAX_CHANGE_IN_COMPETITIVE_POINTS && value <= CompetitionManager.MAX_CHANGE_IN_COMPETITIVE_POINTS )
		{
			int newValue = competitivePoints + value;
			if( newValue  < 1 ) newValue = 1;
			setCompetitivePoints( newValue );
		}
		else
		{
			Debug.LogWarning("PlayerProfile-the number of CP to change " + value + " is incorrect. It needs to be between " + (-CompetitionManager.MAX_CHANGE_IN_COMPETITIVE_POINTS).ToString() + " and " + CompetitionManager.MAX_CHANGE_IN_COMPETITIVE_POINTS.ToString() + ".");
		}
	}

	//Use changeCompetitivePoints. Do not call this method directly (except for the debug menu call).
	public void setCompetitivePoints( int value )
	{
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Competitive_Points_Changed, competitivePoints, value );

		competitivePoints = value;
		if( competitivePoints == 0 )
		{
			setCompletedTutorial( false );
		}
		else
		{
			setCompletedTutorial( true );
		}
		GameManager.Instance.playerStatistics.setHighestNumberOfCompetitivePoints( competitivePoints );
		//Verify if the player changed sector because of the change in CP.
		verifyIfSectorChanged();
		Debug.Log("PlayerProfile-setCompetitivePoints to: " + value );
	}

	public void verifyIfSectorChanged()
	{
		int sectorAfterCPChange = SectorManager.Instance.getSectorByPoints( competitivePoints );

		if( sectorAfterCPChange > currentSector )
		{
			if( sectorAfterCPChange > highestSector )
			{
				if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_UP_AND_NEW, currentSector, sectorAfterCPChange );
			}
			else
			{
				if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_UP, currentSector, sectorAfterCPChange );
			}
		}
		else if( sectorAfterCPChange < currentSector )
		{
			if( sectorChanged != null ) sectorChanged( SectorStatus.WENT_DOWN, currentSector, sectorAfterCPChange );
		}
		setCurrentSector( sectorAfterCPChange );
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
			serializePlayerprofile( true );
			Debug.Log("PlayerProfile-setting current sector to: " + value + " Highest Sector is: " + highestSector );
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
		return ProgressionManager.Instance.getLevel( totalXPEarned );
	}

	public int getTotalXPEarned()
	{
		return totalXPEarned;
	}

	public void addToTotalXPEarned( int xpAmount, bool saveImmediately )
	{
		if( xpAmount > 0 && xpAmount <= ProgressionManager.MAX_XP_IN_ONE_RACE )
		{
			int previousAmount = totalXPEarned;
			totalXPEarned = totalXPEarned + xpAmount;
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.XP_Changed, previousAmount, totalXPEarned );
			if( saveImmediately ) serializePlayerprofile( true );
			Debug.Log("PlayerProfile-addXP: adding XP: " + xpAmount + " New total is: " +  totalXPEarned );
		}
		else
		{
			Debug.LogError("PlayerProfile-the xp amount specified " + xpAmount + " is incorrect. It needs to be between 1 and " + ProgressionManager.MAX_XP_IN_ONE_RACE.ToString() + ".");
		}
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
		serializePlayerprofile( true );
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

	public void serializePlayerprofile( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerProfile( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}
}
