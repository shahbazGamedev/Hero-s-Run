using System.Collections;
using UnityEngine;

[System.Serializable]
public class LootBoxData
{
	public LootBoxType type; 
	public int earnedAtLevel;
	public int earnedInSector;

	public LootBoxData( LootBoxType type, int earnedAtLevel, int earnedInSector )
	{
		this.type = type; 
		this.earnedAtLevel = earnedAtLevel;
		this.earnedInSector = earnedInSector;
	}

	public string ToString()
	{
		return "LootBoxData-LootBoxType: " + type + " Earned at level: " + " Earned in sector: " + earnedInSector;
	}

}
