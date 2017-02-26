﻿using System.Collections;
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
	int[,] numberOfCardsRequiredForUpgrade;
	int[,] coinsRequiredForUpgrade;
	int[,] xpGainedAfterUpgrading;

	void Start()
	{
		initialize();
	}

	void initialize()
	{
		//Common, Rare, Epic, Legendary
		//-1 means Not Applicable
        numberOfCardsRequiredForUpgrade = new int[,]
		{
			{ -1, -1, -1, -1 }, 	//Level 0 is not used
			{ 1, 1, 1, 1 },			//1
			{ 2, 2, 2, 2 },			//2
			{ 4, 4, 4, 4 },			//3
			{ 10, 10, 10, 10 },		//4
			{ 20, 20, 20, 20 },		//5
			{ 50, 50, 50, -1 },		//6
			{ 100, 100, 100, -1 },	//7
			{ 200, 200, 200, -1 },	//8
			{ 400, 400, -1, -1 },	//9
			{ 800, 800, -1, -1 },	//10
			{ 1000, 1000, -1, -1 },	//11
			{ 2000, -1, -1, -1 },	//12
			{ 5000, -1, -1, -1 }	//13
		};

        coinsRequiredForUpgrade = new int[,]
		{
			{ -1, -1, -1, -1 }, 			//Level 0 is not used
			{ -1, -1, -1, -1 },				//1
			{ 5, 50, 400, 5000 },			//2
			{ 20, 150, 2000, 20000 },		//3
			{ 50, 400, 4000, 50000 },		//4
			{ 150, 1000, 8000, 100000 },	//5
			{ 400, 2000, 20000, -1 },		//6
			{ 1000, 4000, 50000, -1 },		//7
			{ 2000, 8000, 100000, -1 },		//8
			{ 4000, 20000, -1, -1 },		//9
			{ 8000, 50000, -1, -1 },		//10
			{ 20000, 100000, -1, -1 },		//11
			{ 50000, -1, -1, -1 },			//12
			{ 100000, -1, -1, -1 }			//13
		};	

        xpGainedAfterUpgrading = new int[,]
		{
			{ -1, -1, -1, -1 }, 	//Level 0 is not used
			{ -1, -1, -1, -1 },		//1
			{ 4, 6, 25, 200 },		//2
			{ 5, 10, 50, 400 },		//3
			{ 6, 25, 100, 1000 },	//4
			{ 10, 50, 200, 1600 },	//5
			{ 25, 100, 400, -1 },	//6
			{ 50, 200, 1000, -1 },	//7
			{ 100, 400, 1600, -1 },	//8
			{ 200, 600, -1, -1 },	//9
			{ 400, 800, -1, -1 },	//10
			{ 600, 1600, -1, -1 },	//11
			{ 800, -1, -1, -1 },	//12
			{ 1600, -1, -1, -1 }	//13
		};	

	}

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
