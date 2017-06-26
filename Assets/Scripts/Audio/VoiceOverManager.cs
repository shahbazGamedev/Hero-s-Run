using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoiceOverType {

	VO_Casting_Spell = 1,
	VO_Resurrect = 2,
	VO_Took_Lead = 3,
	VO_Affected_by_Spell = 4,
	VO_Taunt = 5

}

public class VoiceOverManager : MonoBehaviour {

	[Header("General")]
	public static VoiceOverManager Instance;
	//Key is hero name like McCree, Value is Voice Over Data
	Dictionary<string,List<VoiceOverData>> voiceOverDictionary = new Dictionary<string,List<VoiceOverData>>();
	[SerializeField] List<VoiceOverData> McCreeList = new List<VoiceOverData>();
	[SerializeField] List<VoiceOverData> TracerList = new List<VoiceOverData>();
	Transform player;
	
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
			initialiseDictionary();
		}
	}

	void initialiseDictionary ()
	{
		voiceOverDictionary.Add( "McCree", McCreeList );
		voiceOverDictionary.Add( "Tracer", TracerList );
	}

	#region Taunt
	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}

	public void playTaunt ()
	{
		string heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name;
		//For now, we only have two VO lists: McCree and Tracer
		if( heroName == "Tracer" || heroName == "McCree" )
		{
			//All good
		}
		else
		{
			if( heroName == "Hanzo" || heroName == "Reinhart" )
			{
				//Swap for McCree
				heroName = "McCree";
			}
			else if( heroName == "Mercy" || heroName == "Mei" )
			{
				//Swap for Tracer
				heroName = "Tracer";
			}
		}

		int uniqueId = GameManager.Instance.playerVoiceLines.getEquippedVoiceLineIdForHero( heroName );
		VoiceOverData equipedVoiceLine = getAllTaunts ().Find(vo => vo.uniqueId == uniqueId );
		if( equipedVoiceLine != null )
		{
			player.GetComponent<PlayerVoiceOvers>().playTaunt( equipedVoiceLine.clip, uniqueId );
		}
	}

	public int getRandomTaunt ( string heroName )
	{
		List<VoiceOverData> voiceOverList = getVoiceOverList( heroName );
		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> vodList = voiceOverList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
		if( vodList.Count > 0 )
		{
			if( vodList.Count == 1 )
			{
				return vodList[0].uniqueId;
			}
			else
			{
				//We have multiple entries that match. Let's select a random one.
				int random = Random.Range( 0, vodList.Count );
				return vodList[random].uniqueId;
			}
		}
		Debug.LogError("getRandomTaunt-no taunt found for hero name " + heroName );
		return 0;
	}

	public AudioClip getTauntClip( string heroName, int uniqueId )
    {
		//Find the clip to play
		List<VoiceOverData> voiceOverList = getVoiceOverList( heroName );
		VoiceOverData equippedVoiceLine = voiceOverList.Find(vo => vo.type == VoiceOverType.VO_Taunt && vo.uniqueId == uniqueId );
		return equippedVoiceLine.clip;
	}

	public List<VoiceOverData> getAllTaunts ()
	{
		List<VoiceOverData> allHeroesTauntListList = new List<VoiceOverData>();
		//McCree
		List<VoiceOverManager.VoiceOverData> heroTaunts = McCreeList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
		allHeroesTauntListList.AddRange( heroTaunts );
		//Tracer
		heroTaunts = TracerList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
		allHeroesTauntListList.AddRange( heroTaunts );
		return allHeroesTauntListList;
	}
	#endregion

	public bool doesVoiceLineExist( int uniqueId )
	{
		//NOT IMPLEMENTED
		return true;
	}

	public List<VoiceOverData> getVoiceOverList ( string heroName )
	{
		//For now, we only have two VO lists: McCree and Tracer
		if( voiceOverDictionary.ContainsKey( heroName ) )
		{
			return voiceOverDictionary[heroName];
		}
		else
		{
			if( heroName == "Hanzo" || heroName == "Reinhart" )
			{
				//Return a male voice set
				return voiceOverDictionary["McCree"];
			}
			else if( heroName == "Mercy" || heroName == "Mei" )
			{
				//Return a female voice set
				return voiceOverDictionary["Tracer"];
			}
		}
		return null;
	}

	[System.Serializable]
	public class VoiceOverData
	{
		public int uniqueId;
		public string heroName;
		public VoiceOverType type = VoiceOverType.VO_Casting_Spell;
		//playOnActivationOnly is only used for cards.
		//For some cards (such as Sprint) a VO is automatically played. This is handled by CardHandler.
		//For other cards (such as Stasis), we want the VO (such as "Gotcha!") to only play if a target was found.
		//So, when setting up the VoiceOverData, Sprint would have playOnActivationOnly set to true, but Stasis
		//would have playOnActivationOnly set to false (since the Card class will play the VO if it found a target).
		public bool playOnActivationOnly = true;
		public CardName cardName;
		//Each hero has one default taunt that is immediately available for new players.
		//All other taunts need to be found, for example in loot boxes.
		public bool isDefaultTaunt = false;
		public AudioClip clip;
	}
}
