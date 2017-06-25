using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootType
{
	COINS = 0,
	GEMS = 1,
	CARDS = 2,
	PLAYER_ICON = 3,
	VOICE_LINE = 4
}

[System.Serializable]
public class LootBox {

	[SerializeField] List<Loot> lootList = new List<Loot>();

	public void addLoot( Loot loot )
	{
		lootList.Add( loot );
	}

	public List<Loot> getLootList()
	{
		return lootList;
	}

	public string getJson()
	{
		return JsonUtility.ToJson( this );
	}

	public void print()
	{
		for( int i = 0; i < lootList.Count; i++ )
		{
			lootList[i].print();
		}
	}

	[System.Serializable]
	public class Loot
	{
		public LootType type; 
		public int quantity;
		public CardName cardName;
		public int uniqueItemID;

		public void print()
		{
			Debug.Log("Loot-LootType: " + type.ToString() + " Quantity: " + quantity + " CardName: " + cardName + " UniqueItemID: " + uniqueItemID );
		}
	}
	
}
