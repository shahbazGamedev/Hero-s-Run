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

[System.Serializable]
public class PlayerProfile {

	[SerializeField] string userName;
	public int totalXPEarned = 0;
	public int xpProgressToNextLevel = 0;
	public int level = 1;
	public int prestigeLevel = 0;
	public string lastMatchPlayedTimeString = "01/01/1970 00:00:00";
	public string lastMatchWonTimeString = "01/01/1970 00:00:00";
	private DateTime lastMatchPlayedTime = new DateTime(1970,01,01);
	private DateTime lastMatchWonTime = new DateTime(1970,01,01);
	//This is the unique Id of the player icon.
	//By default, the Id is zero. This is the Id of the default player icon that new players have.
	//The user can change his player icon in the Player Icon screen.
	[SerializeField] int playerIconId = 0;
	public int selectedHeroIndex; //index for heroCharacterList in HeroManager

	[SerializeField] bool completedTutorial = true; //TRUE FOR TESTING
	//Trophies indicate your success in racing. Players gain or lose Trophies by either winning or losing races in online multiplayer races.
	//The number of trophies you have indicate which race track you will be racing in for multiplayer races.
	[SerializeField] int numberOfTrophies = 0;
 	//Not serialized. trophiesEarnedOrLost is set by PlayerRaceManager in the Level scene, but must be read by GameEndManager in the Matchmaking Scene.
	private int trophiesEarnedLastRace = 0;

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
	public bool hasCompletedTutorial()
	{
		return completedTutorial;
	}

	/// <summary>
	/// Saves the fact that the player has completed tutorial.
	/// </summary>
	public void saveHasCompletedTutorial()
	{
		completedTutorial = true;
		serializePlayerprofile();
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

	public void changeTrophies( int value )
	{
		if( value >= -TrophyManager.MAX_CHANGE_IN_TROPHIES && value <= TrophyManager.MAX_CHANGE_IN_TROPHIES )
		{
			int newValue = numberOfTrophies + value;
			//Don't allow the number of trophies to go below 0.
			if( newValue  < 0 ) newValue = 0;
			setNumberOfTrophies( newValue );
		}
		else
		{
			Debug.LogWarning("PlayerProfile-the number of trophies to change " + value + " is incorrect. It needs to be between " + (-TrophyManager.MAX_CHANGE_IN_TROPHIES) + " and " + TrophyManager.MAX_CHANGE_IN_TROPHIES.ToString() + ".");
		}
	}

	public void setNumberOfTrophies( int value )
	{
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Trophies_Changed, numberOfTrophies, value );
		numberOfTrophies = value;
		GameManager.Instance.playerStatistics.setHighestNumberOfTrophies( numberOfTrophies );
		Debug.Log("PlayerProfile-setNumberOfTrophies to: " + value );
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
	}

	public DateTime getLastMatchPlayedTime()
	{
		lastMatchPlayedTime = Convert.ToDateTime (lastMatchPlayedTimeString); 
		return lastMatchPlayedTime; 
	}

	public void setLastMatchWonTime( DateTime timeMatchWon )
	{
		lastMatchWonTime = timeMatchWon;
	}

	public DateTime getLastMatchWonTime()
	{
		lastMatchWonTime = Convert.ToDateTime (lastMatchWonTimeString); 
		return lastMatchWonTime.Date; //We don't want the time element, only the date
	}

	public void setPlayerIconId( int value )
	{
		if( value >= 0 && value < ProgressionManager.Instance.getNumberOfPlayerIcons() )
		{
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Player_Icon_Changed, playerIconId, value );
			playerIconId = value;
		}
		else
		{
			Debug.LogWarning("PlayerProfile-setPlayerIconId: the value specified," + value + " is incorrect. It needs to be between 0 and " + ProgressionManager.Instance.getNumberOfPlayerIcons() + " exclusively." );
		}
	}

	public int getPlayerIconId()
	{
		return playerIconId;
	}

	public void serializePlayerprofile()
	{
		lastMatchPlayedTimeString = lastMatchPlayedTime.ToString();
		lastMatchWonTimeString = lastMatchWonTime.ToString();
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerProfile( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}
}
