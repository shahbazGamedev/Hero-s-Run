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

public class HeroManager : MonoBehaviour {

	[Header("General")]
	public static HeroManager Instance;
	[SerializeField] List<HeroCharacter> heroCharacterList = new List<HeroCharacter>();
	[SerializeField] List<BotHeroCharacter> botHeroCharacterList = new List<BotHeroCharacter>();
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
		[Range(1, 10 )]
		public int skillLevel;		//How skillfull is the bot. 1 being very clumsy and 10 being amazing.
	}

}
