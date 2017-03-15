using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityEffect
{
	//Passive
	RUN_FASTER = 1,
	JUMP_HIGHER = 2,
	NEVER_STUMBLE = 3,
	DEFLECT_MISSILES = 4,
	FEATHER_FALL = 5,
	//Active
	BLINK = 100,
	LIGHTNING_STRIKE = 101,
	CHILL_TOUCH = 102,
	WALL_OF_FIRE = 103,
	TRANSMOGRIFY = 104,
	WALL_OF_ICE = 105
}

public enum Sex
{
	MALE = 0,
	FEMALE = 1
}

public enum BotSkillLevel
{
	VERY_LOW = -1,
	LOW = 0,
	MEDIUM = 1,
	HIGH = 2
}

public class HeroManager : MonoBehaviour {

	[Header("General")]
	public static HeroManager Instance;
	[SerializeField] List<HeroCharacter> heroCharacterList = new List<HeroCharacter>();
	[SerializeField] List<BotHeroCharacter> botHeroCharacterList = new List<BotHeroCharacter>();
	Dictionary<BotSkillLevel, BotSkillData> botSkillDataDictionary = new Dictionary<BotSkillLevel, BotSkillData>(3);
	//Each character has an ACTIVE ability (such as blink) and a PASSIVE ability (such as jump higher)
	[SerializeField] List<HeroAbility> heroAbilityList = new List<HeroAbility>();

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
			initialiseBotSkillData();
		}
	}

	public HeroCharacter getHeroCharacter( int index )
	{
		return heroCharacterList[index];
	}

	public int getNumberOfHeroes()
	{
		return heroCharacterList.Count;
	}

	public HeroAbility getHeroAbility( AbilityEffect abilityEffect )
	{
		return heroAbilityList.Find(ability => ability.abilityEffect == abilityEffect);
	}

	#region Bot related

	void initialiseBotSkillData()
	{
		//Very Low Skill (the bot never plays cards)
		BotSkillData botSkillData = new BotSkillData( 0, 0, 0.9f, 0.9f );
		botSkillDataDictionary.Add(BotSkillLevel.VERY_LOW, botSkillData );
		//Low Skill
		botSkillData = new BotSkillData( 8, 24, 0.9f, 0.92f );
		botSkillDataDictionary.Add(BotSkillLevel.LOW, botSkillData );
		//Medium Skill
		botSkillData = new BotSkillData( 4, 16, 0.95f, 0.95f );
		botSkillDataDictionary.Add(BotSkillLevel.MEDIUM, botSkillData );
		//High Skill
		botSkillData = new BotSkillData( 2, 8, 0.99f, 0.99f );
		botSkillDataDictionary.Add(BotSkillLevel.HIGH, botSkillData );
	}

	public BotSkillData getBotSkillData( BotSkillLevel botSkillLevel )
	{
		if( botSkillDataDictionary.ContainsKey(botSkillLevel) )
		{
			return botSkillDataDictionary[botSkillLevel];
		}
		else
		{
			Debug.LogError("HeroManager-There is no entry in the bot skill dictionary for " + botSkillLevel );
			return null;
		}
	}

	public BotHeroCharacter getBotHeroCharacter( int index )
	{
		return botHeroCharacterList[index];
	}

	public int getIndexOfOppositeSexBot( Sex sex )
	{
		int heroOfOppositeSex = 0;
		for( int i = 0; i < botHeroCharacterList.Count; i++ )
		{
			if( botHeroCharacterList[i].sex != sex ) return i;
		}
		return heroOfOppositeSex;
	}
	#endregion

	[System.Serializable]
	public class HeroAbility
	{
		public AbilityEffect abilityEffect = AbilityEffect.RUN_FASTER;
		public enum AbilityType
		{
			ACTIVE = 1,
			PASSIVE = 2,
		}
		public Sprite icon;
		public AbilityType type = AbilityType.ACTIVE;
	}

	[System.Serializable]
	public class HeroCharacter
	{
		public string name;
		public Sex sex;
 		//See heroSkinList in HeroCarousel for how skinIndex is used.
		//The skins are stored in the Hero Selection scene with the correct position, rotation and scale.
		public int skinIndex;
		public string skinPrefab;
		public Sprite icon;
		public AbilityEffect activeAbilityEffect;
		public AbilityEffect passiveAbilityEffect;
		public Sprite minimapIcon;
	}

	[System.Serializable]
	/// <summary>
	/// Bot hero character. Used when in the PlayAgainstEnemy mode.
	/// </summary>
	public class BotHeroCharacter : HeroCharacter
	{
		public string userName; 	//Bot name displayed is matchmaking lobby
 		public int playerIcon;		//Bot icon displayed is matchmaking lobby
		public int level;			//A number between 1 and 100. Only affects the frame to use in the matchmaking screen.
		public BotSkillLevel skillLevel;
		public List<PlayerDeck.PlayerCardData> botCardDataList;
	}

	[System.Serializable]
	/// <summary>
	/// Used to define how skillfull the bot is.
	/// </summary>
	public class BotSkillData
	{
		public float cardPlayFrequency; 				//How often, in seconds, will the bot consider playing a card.
 		public float raceStartGracePeriod;				//How long will the bot wait in seconds before playing cards.
		public float percentageWillTurnSuccesfully;		//How frequently will the bot turn a corner successfully.
		public float percentageWillTryToAvoidObstacle;	//How frequently will the bot try to avoid the obstacle in front of him.

		public BotSkillData( float cardPlayFrequency, float raceStartGracePeriod, float percentageWillTurnSuccesfully, float percentageWillTryToAvoidObstacle )
		{
			this.cardPlayFrequency = cardPlayFrequency;
			this.raceStartGracePeriod = raceStartGracePeriod;
			this.percentageWillTurnSuccesfully = percentageWillTurnSuccesfully;
			this.percentageWillTryToAvoidObstacle = percentageWillTryToAvoidObstacle;
		}
	}

}
