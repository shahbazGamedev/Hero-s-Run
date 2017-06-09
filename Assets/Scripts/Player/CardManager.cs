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

public enum CardPropertyType
 {
	DURATION = 0,
	RANGE = 1,
	DOUBLE_JUMP_SPEED = 2,
	RADIUS = 3,
	ACCURACY = 4,
	RUN_SPEED = 5,
	AIM_RANGE = 6,
	DURATION_WITH_TIMER = 7,
	TARGET = 8
}

public enum CardPropertyTargetType
 {
	NOT_APPLICABLE = 0,
	NEAREST = 1,
	RANDOM = 2,
	ALL = 3,
}

public enum CardSortMode
{
	BY_RARITY = 0,
	BY_MANA_COST = 1
}



/// <summary>
/// Card name.
/// Use numbers 0 to 99 for Common, 100 to 199 for Rare, 200 to 299 for Epic and 300 to 399 for Legendary.
/// </summary>
public enum CardName
{
	None = 0,

	//Common
	Raging_Bull = 1,
	Double_Jump = 2,
	Stasis = 3,
	Sprint = 4,

	//Rare
	Firewall = 100,
	Grenade = 101,
	Shrink = 102,
	Trip_Mine = 103,
	Sentry = 104,
	IceWall = 105,
	Steal = 106,

	//Epic
	Lightning = 200,
	Linked_Fate = 201,
	Supercharger = 202,
	Homing_Missile = 203,

	//Legendary
	Hack = 301,
	Reflect = 302,
	Card_Three = 303,
	Card_Four = 304
}

public class CardManager : MonoBehaviour {

	public static CardManager Instance;
	public const int CARDS_IN_TURN_RIBBON = 4;
	[SerializeField] List<CardData> cardDataList = new List<CardData>();
	[SerializeField] List<CardPropertyIcon> cardPropertyIconList = new List<CardPropertyIcon>();
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
	}

	public int getMaxCardLevelForThisRarity( CardRarity rarity )
	{
		int maxCardLevelForThisRarity = 0;
    	switch (rarity)
		{
	        case CardRarity.COMMON:
				maxCardLevelForThisRarity = 13;
                break;
	                
	        case CardRarity.RARE:
				maxCardLevelForThisRarity = 11;
                break;
               
	        case CardRarity.EPIC:
				maxCardLevelForThisRarity = 8;
				break;                

	        case CardRarity.LEGENDARY:
				maxCardLevelForThisRarity = 5;
				break;                
		}
		return maxCardLevelForThisRarity;
	}

	void initialize()
	{
		//Common, Rare, Epic, Legendary
		//-1 means Not Applicable
        numberOfCardsRequiredForUpgrade = new int[,]
		{
			{ -1, -1, -1, -1 }, 	//Level 0 is not used
			{ -1, -1, -1, -1 },		//1
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

	public bool doesCardExist( CardName name )
	{
		return cardDataList.Exists(cardData => cardData.name == name );
	}

	public List<CardData> getAllCards()
	{
		return cardDataList;
	}

	public int getTotalNumberOfCards()
	{
		return cardDataList.Count;
	}

	public CardName getRandomCard( int currentRaceTrackLevel, CardRarity rarity )
	{
		List<CardData> possibleRandomCardList = cardDataList.FindAll(cardData => cardData.rarity == rarity && cardData.raceTrackLevel <= currentRaceTrackLevel );
		Debug.LogWarning("CardManager-getRandomCard: possibleRandomCardList.Count " +  possibleRandomCardList.Count );
		if( possibleRandomCardList.Count > 0 )
		{
			int randomNumber = Random.Range( 0, possibleRandomCardList.Count );
			return possibleRandomCardList[randomNumber].name;
		}
		else
		{
			Debug.LogError("CardManager-getRandomCard: No card was found. Returning CardName.None.");
			return CardName.None;
		}
	}

	public CardData getCardByName( CardName name )
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
	/// Gets the card color hex value. See Rich Text for details.
	/// </summary>
	/// <returns>The card color hex value.</returns>
	/// <param name="rarity">Rarity.</param>
	public string getCardColorHexValue( CardRarity rarity )
	{
		string cardColor = "#ffffffff";
    	switch (rarity)
		{
	        case CardRarity.COMMON:
				cardColor = "#ffff00ff"; //yellow
                break;
	                
	        case CardRarity.RARE:
				cardColor = "#ffa500ff"; //orange
                break;
               
	        case CardRarity.EPIC:
				cardColor = "#00ff00ff"; //lime
				break;                

	        case CardRarity.LEGENDARY:
				cardColor = "#800080ff"; //purple
				break;                
		}
		return cardColor;
	}

	public Sprite getCardPropertyIcon( CardPropertyType type )
	{
		CardPropertyIcon cardPropertyIcon = cardPropertyIconList.Find(property => property.type == type);
		if( cardPropertyIcon != null )
		{
			return cardPropertyIcon.icon;
		}
		else
		{
			Debug.LogError("CardManager-Could not find icon for property " + type );
			return null;
		}
	}

	public string getCardPropertyValueType( CardPropertyType type )
	{
   		switch (type)
		{
	        case CardPropertyType.DURATION:
	             return "Sec.";
	                
			case CardPropertyType.RANGE:
 			case CardPropertyType.AIM_RANGE:
 			case CardPropertyType.RADIUS:
 	             return "M";
               
			default:
				return string.Empty;               
		}
	}

	/// <summary>
	/// Card data. The card data only handles data that never changes.
	/// </summary>
	[System.Serializable]
	public class CardData
	{
		//Unique name to identify the card.
		public CardName name; 
		public CardRarity rarity = CardRarity.COMMON;
		public Sprite icon;
		public List<CardProperty> cardProperties;
		//The secondary icon is optional.
		//It appears on the top-left corner of the player icon using or affected by a card.
		//Double Jump doesn't have a secondary icon, but Sentry does for example.
		public Sprite secondaryIcon;
		[Range(1,9)]
		public int manaCost;
		//The race track level where training track is 0, the first race track unlocked is 1, the second race track unlocked is 2, etc.
		public int raceTrackLevel;
		[System.NonSerialized]
		public RectTransform rectTransform;
		[System.NonSerialized]
		public bool isStolenCard;
		//Legendary cards have special effects on them
		public Material cardMaterial;

		public float getCardPropertyValue( CardPropertyType type, int level )
		{
			float cardPropertyValue = 0;
			CardProperty cardProperty = cardProperties.Find(property => property.type == type);
			if( cardProperty != null )
			{
				if( level > CardManager.Instance.getMaxCardLevelForThisRarity( rarity ) )
				{
					Debug.LogError("CardManager-The level specified for the card " + name + " is too high. Using maximum allowed value instead.");
					level = CardManager.Instance.getMaxCardLevelForThisRarity( rarity );
					cardPropertyValue = cardProperty.baseValue + level * cardProperty.upgradeValue;
				}
				else
				{
					cardPropertyValue = cardProperty.baseValue + level * cardProperty.upgradeValue;
				}
			}
			else
			{
				Debug.LogError("CardManager-The card " + name + " does not have the " + type + " property.");
			}
			return cardPropertyValue;
		}

		public CardPropertyTargetType getCardPropertyTargetType()
		{
			CardProperty cardProperty = cardProperties.Find(property => property.type == CardPropertyType.TARGET );
			if( cardProperty != null )
			{
				return cardProperty.targetType;
			}
			else
			{
				Debug.LogError("CardManager-The card " + name + " does not have the " + CardPropertyType.TARGET.ToString() + " property.");
				return CardPropertyTargetType.NOT_APPLICABLE;
			}
		}

		public bool doesCardHaveThisProperty( CardPropertyType type )
		{
			return cardProperties.Exists(property => property.type == type);
		}
	}

	[System.Serializable]
	public class CardProperty
	{
		public CardPropertyType type; 
		public float baseValue;
		public float upgradeValue;
		public CardPropertyTargetType targetType;
	}

	[System.Serializable]
	public class CardPropertyIcon
	{
		public CardPropertyType type; 
		public Sprite icon;
	}

}
