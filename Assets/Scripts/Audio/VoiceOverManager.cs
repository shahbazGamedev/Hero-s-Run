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
	[SerializeField] List<VoiceOverData> voiceOverList = new List<VoiceOverData>();
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
		}
	}

	#region Taunt
	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}

	public void playTaunt ()
	{
		string heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name;
		int uniqueId = GameManager.Instance.playerVoiceLines.getEquippedVoiceLineIdForHero( heroName );
		VoiceOverData equipedVoiceLine = getAllTaunts ().Find(vo => vo.uniqueId == uniqueId );
		if( equipedVoiceLine != null )
		{
			player.GetComponent<PlayerVoiceOvers>().playTaunt( equipedVoiceLine.clip, uniqueId );
		}
	}

	//To do: remove default taunts for loot boxes
	public int getRandomTaunt ( string heroName )
	{
		List<VoiceOverData> heroVoiceOverList = getHeroVoiceOverList( heroName );
		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> tauntList = heroVoiceOverList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
		if( tauntList.Count > 0 )
		{
			if( tauntList.Count == 1 )
			{
				return tauntList[0].uniqueId;
			}
			else
			{
				//We have multiple entries that match. Let's select a random one.
				int random = Random.Range( 0, tauntList.Count );
				return tauntList[random].uniqueId;
			}
		}
		Debug.LogError("getRandomTaunt-no taunt found for hero name " + heroName );
		return 0;
	}

	public AudioClip getTauntClip( string heroName, int uniqueId )
    {
		//Find the clip to play
		List<VoiceOverData> heroVoiceOverList = getHeroVoiceOverList( heroName );
		VoiceOverData equippedVoiceLine = heroVoiceOverList.Find(vo => vo.type == VoiceOverType.VO_Taunt && vo.uniqueId == uniqueId );
		return equippedVoiceLine.clip;
	}

	public List<VoiceOverData> getAllTaunts ()
	{
		return voiceOverList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
	}
	#endregion

	public bool doesVoiceLineExist( int uniqueId )
	{
		return voiceOverList.Exists(vo => ( vo.uniqueId == uniqueId ) );
	}

	public List<VoiceOverData> getHeroVoiceOverList ( string heroName )
	{
		return voiceOverList.FindAll(vo => ( vo.heroName == heroName ) );
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
