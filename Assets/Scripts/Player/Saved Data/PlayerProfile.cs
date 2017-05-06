using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerProfileEvent {
	Level_Changed = 0,
	Player_Icon_Changed = 1,
	XP_Changed = 2,
	User_Name_Changed = 3
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

	//Delegate used to communicate to other classes when the a value changes
	public delegate void PlayerProfileChanged( PlayerProfileEvent eventType, int previousValue = 0, int newValue = 0 );
	public static event PlayerProfileChanged playerProfileChanged;

	public string getUserName()
	{
		return userName;
	}
	
	public void saveUserName( string value )
	{
		userName = value;
		if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.User_Name_Changed );
		serializePlayerprofile();
	}

	public int getLevel()
	{
		return level;
	}

	public void setLevel( int value )
	{
		if( value > 0 && value <= ProgressionManager.MAX_LEVEL )
		{
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Level_Changed, level, value );
			level = value;
			Debug.Log("PlayerProfile-setLevel: setting level to: " + value );
		}
		else
		{
			Debug.LogWarning("PlayerProfile-setLevel: the level specified," + value + " is incorrect. It needs to be between 1 and 100.");
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
			playerIconId = value;
			if( playerProfileChanged != null ) playerProfileChanged( PlayerProfileEvent.Player_Icon_Changed, playerIconId );
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
