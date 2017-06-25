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
		voiceOverDictionary.Add( "Male", McCreeList );
		voiceOverDictionary.Add( "Female", TracerList );
	}

	#region Taunt
	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}

	public void playTaunt ()
	{
		Sex sex = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).sex;
		List<VoiceOverData> voiceOverList = getVoiceOverList( sex );
		int voiceLineId = GameManager.Instance.playerProfile.getVoiceLineId();
		VoiceOverData equipedVoiceLine = voiceOverList.Find(vo => ( vo.type == VoiceOverType.VO_Taunt && vo.voiceLineId == voiceLineId ) );
		if( equipedVoiceLine != null )
		{
			player.GetComponent<PlayerVoiceOvers>().playTaunt( equipedVoiceLine.clip, sex, voiceLineId );
		}
	}

	public int getRandomTaunt ( Sex sex )
	{
		List<VoiceOverData> voiceOverList = getVoiceOverList( sex );
		//Do we have one or more VOs that match?
		List<VoiceOverManager.VoiceOverData> vodList = voiceOverList.FindAll(vo => ( vo.type == VoiceOverType.VO_Taunt ) );
		if( vodList.Count > 0 )
		{
			if( vodList.Count == 1 )
			{
				return vodList[0].voiceLineId;
			}
			else
			{
				//We have multiple entries that match. Let's select a random one.
				int random = Random.Range( 0, vodList.Count );
				return vodList[random].voiceLineId;
			}
		}
		Debug.LogError("getRandomTaunt-no taunt found for sex " + sex );
		return 0;
	}

	public AudioClip getTauntClip( Sex sex, int voiceLineId )
    {
		//Find the clip to play
		List<VoiceOverData> voiceOverList = getVoiceOverList( (Sex) sex );
		VoiceOverData equippedVoiceLine = voiceOverList.Find(vo => vo.type == VoiceOverType.VO_Taunt && vo.voiceLineId == voiceLineId );
		return equippedVoiceLine.clip;
	}
	#endregion

	public List<VoiceOverData> getVoiceOverList ( Sex sex )
	{
		if( sex == Sex.MALE )
		{
			return voiceOverDictionary["Male"];
		}
		else
		{
			return voiceOverDictionary["Female"];
		}
	}
	
	public List<VoiceOverData> getVoiceOverList ( string listName )
	{
		if( voiceOverDictionary.ContainsKey(listName) )
		{
			return voiceOverDictionary[listName];
		}
		else
		{
			Debug.LogError("VoiceOverManager-the voice over dictionary doesn't contain an entry for " + listName );
			return null;
		}
	}

	[System.Serializable]
	public class VoiceOverData
	{
		public int voiceLineId;
		public VoiceOverType type = VoiceOverType.VO_Casting_Spell;
		//playOnActivationOnly is only used for cards.
		//For some cards (such as Sprint) a VO is automatically played. This is handled by CardHandler.
		//For other cards (such as Stasis), we want the VO (such as "Gotcha!") to only play if a target was found.
		//So, when setting up the VoiceOverData, Sprint would have playOnActivationOnly set to true, but Stasis
		//would have playOnActivationOnly set to false (since the Card class will play the VO if it found a target).
		public bool playOnActivationOnly = true;
		public CardName cardName; 
		public AudioClip clip;
	}
}
