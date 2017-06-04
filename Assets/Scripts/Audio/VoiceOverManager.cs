using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoiceOverType {

	VO_Casting_Spell = 1,
	VO_Resurrect = 2,
	VO_Took_Lead = 3,
	VO_Affected_by_Spell = 4

}

public class VoiceOverManager : MonoBehaviour {

	[Header("General")]
	public static VoiceOverManager Instance;
	//Key is hero name like McCree, Value is Voice Over Data
	Dictionary<string,List<VoiceOverData>> voiceOverDictionary = new Dictionary<string,List<VoiceOverData>>();
	[SerializeField] List<VoiceOverData> McCreeList = new List<VoiceOverData>();
	[SerializeField] List<VoiceOverData> TracerList = new List<VoiceOverData>();
	
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
