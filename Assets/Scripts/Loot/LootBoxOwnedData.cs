using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class LootBoxOwnedData
{
	public LootBoxType type; 
	public int earnedAtLevel;
	public int earnedInBase;
	public LootBoxState state = LootBoxState.NOT_INITIALIZED;
	public string lastFreeLootBoxOpenedTimeString = "01/01/1970 00:00:00";
	private DateTime lastFreeLootBoxOpenedTime = new DateTime(1970,01,01);
	public string unlockStartTimeString = "01/01/1970 00:00:00";
	private DateTime unlockStartTime = new DateTime(1970,01,01);

	public LootBoxOwnedData( LootBoxType type, int earnedAtLevel, int earnedInBase )
	{
		this.type = type; 
		this.earnedAtLevel = earnedAtLevel;
		this.earnedInBase = earnedInBase;
	}

	public LootBoxOwnedData( LootBoxType type, int earnedInBase, LootBoxState state )
	{
		this.type = type; 
		this.earnedInBase = earnedInBase;
		this.state = state;
	}

	public void setLastFreeLootBoxOpenedTime( DateTime lastFreeLootBoxOpenedTime )
	{
		this.lastFreeLootBoxOpenedTime = lastFreeLootBoxOpenedTime;
		lastFreeLootBoxOpenedTimeString = lastFreeLootBoxOpenedTime.ToString();
	}

	public DateTime getLastFreeLootBoxOpenedTime()
	{
		lastFreeLootBoxOpenedTime = Convert.ToDateTime (lastFreeLootBoxOpenedTimeString); 
		return lastFreeLootBoxOpenedTime;
	}

	public void setUnlockStartTime( DateTime unlockStartTime )
	{
		this.unlockStartTime = unlockStartTime;
		unlockStartTimeString = unlockStartTime.ToString();
	}

	public DateTime getUnlockStartTimeTime()
	{
		unlockStartTime = Convert.ToDateTime (unlockStartTimeString); 
		return unlockStartTime;
	}

	public string ToString()
	{
		return "LootBoxOwnedData-LootBoxType: " + type + " Earned at level: " + " Earned in base: " + earnedInBase;
	}

}
