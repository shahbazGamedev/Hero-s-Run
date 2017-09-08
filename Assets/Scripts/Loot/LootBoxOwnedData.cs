using System.Collections;
using UnityEngine;

[System.Serializable]
public class LootBoxOwnedData
{
	public LootBoxType type; 
	public int earnedAtLevel;
	public int earnedInSector;

	public LootBoxOwnedData( LootBoxType type, int earnedAtLevel, int earnedInSector )
	{
		this.type = type; 
		this.earnedAtLevel = earnedAtLevel;
		this.earnedInSector = earnedInSector;
	}

	public string ToString()
	{
		return "LootBoxOwnedData-LootBoxType: " + type + " Earned at level: " + " Earned in sector: " + earnedInSector;
	}

}
