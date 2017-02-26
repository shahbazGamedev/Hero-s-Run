using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardRarity
 {
	COMMON = 1,
	RARE = 2,
	EPIC = 3,
	LEGENDARY = 4
	
}

public enum CardType
 {
	SPEED = 1,
	ATTACK = 2,
	DEFEND = 3	
}

public class CardManager : MonoBehaviour {

	[SerializeField] List<CardData> cardDataList = new List<CardData>();
	
	/// <summary>
	/// Card data. The card data only handles data that never changes.
	/// </summary>
	[System.Serializable]
	public class CardData
	{
		//Unique name to identify the card.
		public string name; 
		public CardRarity rarity = CardRarity.COMMON;
		public CardType type;
		public Sprite icon;
		public string descriptionId;
		[Range(1,7)]
		public int manaCost;		
	}
}
