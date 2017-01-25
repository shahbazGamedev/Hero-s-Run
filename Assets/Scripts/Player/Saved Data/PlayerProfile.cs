using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class PlayerProfile {

	public int currentXP = 0;
	public string lastMatchPlayedTimeString = string.Empty;
	public string lastMatchWonTimeString = string.Empty;
	private DateTime lastMatchPlayedTime = new DateTime(1970,01,01);
	private DateTime lastMatchWonTime = new DateTime(1970,01,01);

	public void addXP( int xpAmount, bool saveImmediately )
	{
		if( xpAmount <= 0 || xpAmount > XPManager.MAX_XP_IN_ONE_RACE ) return;	
		currentXP = currentXP + xpAmount;
		if( saveImmediately ) serializePlayerprofile();
		Debug.Log("PlayerProfile-addXP: adding XP: " + xpAmount + " New total is: " +  currentXP );
	}

	public void setLastMatchPlayedTime( DateTime timeMatchPlayed )
	{
		lastMatchPlayedTime = timeMatchPlayed;
	}

	public DateTime getLastMatchPlayedTime()
	{
		if ( lastMatchPlayedTime == null)
		{ 
			lastMatchPlayedTime = Convert.ToDateTime (lastMatchPlayedTimeString); 
		} 
		return lastMatchPlayedTime; 
	}

	public void setLastMatchWonTime( DateTime timeMatchWon )
	{
		lastMatchWonTime = timeMatchWon;
	}

	public DateTime getLastMatchWonTime()
	{
		if ( lastMatchWonTime == null)
		{ 
			lastMatchWonTime = Convert.ToDateTime (lastMatchWonTimeString); 
		} 
		return lastMatchWonTime.Date; //We don't want the time element, only the date
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
