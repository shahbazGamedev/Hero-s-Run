using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class LootBoxOwnedData
{
	public LootBoxType type; 
	public int earnedAtLevel;
	public int earnedInSector;
	public string lastFreeLootBoxOpenedTimeString = "01/01/1970 00:00:00";
	private DateTime lastFreeLootBoxOpenedTime = new DateTime(1970,01,01);

	public LootBoxOwnedData( LootBoxType type, int earnedAtLevel, int earnedInSector )
	{
		this.type = type; 
		this.earnedAtLevel = earnedAtLevel;
		this.earnedInSector = earnedInSector;
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

	public string ToString()
	{
		return "LootBoxOwnedData-LootBoxType: " + type + " Earned at level: " + " Earned in sector: " + earnedInSector;
	}

}
