using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootType
 {
	COINS = 0,
	GEMS = 1,
	CARDS = 2,
	PLAYER_ICON = 3
}

[System.Serializable]
public class LootBox {

	[SerializeField] List<Loot> LootList = new List<Loot>();

	[System.Serializable]
	public class Loot
	{
		public LootType type; 
		public int quantity;
		public CardName cardName;
		public int uniqueItemID;
	}
	
}
