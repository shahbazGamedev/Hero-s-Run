using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoiceOverType {

	VO_Spell = 1,
	VO_Resurrect = 2,
	VO_Took_Lead = 3
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
		public VoiceOverType type = VoiceOverType.VO_Spell;
		public bool playOnActivationOnly = true;
		public CardName cardName; 
		public AudioClip clip;
	}
}
