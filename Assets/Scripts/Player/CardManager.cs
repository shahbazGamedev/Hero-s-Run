using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardRarity
 {
	COMMON = 0,
	RARE = 1,
	EPIC = 2,
	LEGENDARY = 3
	
}

public enum CardType
 {
	SPEED = 1,
	ATTACK = 2,
	DEFEND = 3	
}

public class CardManager : MonoBehaviour {

	public static CardManager Instance;
	[Range(3,4)]
	public int cardsInTurnRibbon = 4; //Number of cards appearing in the turn-ribbon. We will test with both 3 and 4 and see which one feels best.
	[SerializeField] List<CardData> cardDataList = new List<CardData>();
	int[,] numberOfCardsRequiredForUpgrade;
	int[,] coinsRequiredForUpgrade;
	int[,] xpGainedAfterUpgrading;

	// Use this for initialization
	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	void Start()
	{
		initialize();
		//GameManager.Instance.playerDeck.initialiseForTesting();
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

	public bool doesCardExist( string name )
	{
		return cardDataList.Exists(cardData => cardData.name == name );
	}

	public CardData getCardByName( string name )
	{
		if( doesCardExist( name ) )
		{
			return cardDataList.Find(cardData => cardData.name == name);
		}
		else
		{
			Debug.LogError("CardManager-getCardByName: The card you requested does not exist: " + name );
			return null;
		}
	}

	public int getNumberOfCardsRequiredForUpgrade( int currentCardLevel, CardRarity rarity )
	{
		return numberOfCardsRequiredForUpgrade[currentCardLevel, (int) rarity ];
	}

	public int getCoinsRequiredForUpgrade( int currentCardLevel, CardRarity rarity )
	{
		return coinsRequiredForUpgrade[currentCardLevel, (int) rarity ];
	}

	public int getXPGainedAfterUpgrading( int currentCardLevel, CardRarity rarity )
	{
		return xpGainedAfterUpgrading[currentCardLevel, (int) rarity ];
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
		[Range(1,9)]
		public int manaCost;		
	}

}
