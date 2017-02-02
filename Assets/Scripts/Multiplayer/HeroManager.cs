using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityEffect
{
	//Passive
	RUN_FASTER = 1,
	JUMP_HIGHER = 2,
	NEVER_STUMBLE = 3,
	IMMUNE_TO_MISSILES = 4,
	//Active
	BLINK = 100,
	LIGHTNING_STRIKE = 101
}

public class HeroManager : MonoBehaviour {

	[Header("General")]
	const int NUMBER_OF_CHARACTERS = 6;
	public static HeroManager Instance;
	[SerializeField] List<HeroCharacter> heroCharacterList = new List<HeroCharacter>(NUMBER_OF_CHARACTERS);
	//Each character has an ACTIVE ability (such as blink) and a PASSIVE ability (such as jump higher)
	[SerializeField] List<HeroAbility> heroAbilityList = new List<HeroAbility>(NUMBER_OF_CHARACTERS*2);

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
		public GameObject skin;
		public Sprite icon;
		public AbilityEffect activeAbilityEffect;
		public AbilityEffect passiveAbilityEffect;
	}
}
