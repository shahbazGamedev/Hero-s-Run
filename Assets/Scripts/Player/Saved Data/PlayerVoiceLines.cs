using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PlayerVoiceLines {

	[SerializeField] List<VoiceLineData> voiceLineList = new List<VoiceLineData>();
	
	/// <summary>
	/// Creates the voice lines for a new player.
	/// </summary>
	public void createNewVoiceLines()
	{
		List<VoiceOverManager.VoiceOverData> voiceOverList = VoiceOverManager.Instance.getAllTaunts ();
		for( int i = 0; i < voiceOverList.Count; i++ )
		{
			addVoiceLine( voiceOverList[i].uniqueId, voiceOverList[i].heroName, voiceOverList[i].clip.name, false, !voiceOverList[i].isDefaultTaunt, voiceOverList[i].isDefaultTaunt  );
		}
		serializeVoiceLines( true );
	}

	void addVoiceLine( int uniqueId, string heroName, string clipName, bool isNew, bool isLocked, bool isEquipped )
	{
		//Make sure the specified voice line exists
		if( VoiceOverManager.Instance.doesVoiceLineExist( uniqueId ) )
		{
			VoiceLineData vld = new VoiceLineData();
			vld.uniqueId = uniqueId;
			vld.heroName = heroName;
			vld.clipName = clipName;
			vld.isNew = isNew;
			vld.isLocked = isLocked;
			vld.isEquipped = isEquipped;
			voiceLineList.Add(vld);
		}
		else
		{
			Debug.LogError("PlayerVoiceLines-addVoiceLine: The voice line you are trying to add does not exist: " + uniqueId );
		}
	}

	//Sort the list starting with the newly unlocked icons
	public List<VoiceLineData> getSortedVoiceLineList()
	{
		return voiceLineList.OrderByDescending(data=>data.isNew).ToList();
	}

	public VoiceLineData getVoiceLineDataByUniqueId( int uniqueId )
	{
		return voiceLineList.Find(voiceLineData => voiceLineData.uniqueId == uniqueId);
	}

	public int getUnlockedCountForHero( string heroName )
	{
		return voiceLineList.FindAll(voiceLineData => voiceLineData.heroName == heroName && voiceLineData.isLocked == false ).Count;
	}

	public List<VoiceLineData> getVoiceLinesForHero( string heroName )
	{
		return voiceLineList.FindAll(voiceLineData => voiceLineData.heroName == heroName );
	}

	public int getNumberOfNewVoiceLines()
	{
		int counter = 0;
		for( int i = 0; i < voiceLineList.Count; i++ )
		{
			if( voiceLineList[i].isNew ) counter++;
		}
		return counter;
	}

	public void equipVoiceLine( int uniqueId, string heroName )
	{
		VoiceLineData vld = voiceLineList.Find(voiceLineData => voiceLineData.uniqueId == uniqueId);
		if( vld != null )
		{
			//We only ever want one equipped VO per hero at any given time
			unquippedVoiceLineForHero( heroName );
			vld.isEquipped = true;
		}
		else
		{
			Debug.LogWarning("The voice line with id " + uniqueId + " that you want to equip could not be found." );
		}
	}

	public void unlockVoiceLineAndSave( int uniqueId )
	{
		VoiceLineData vld = voiceLineList.Find(voiceLineData => voiceLineData.uniqueId == uniqueId);
		if( vld != null )
		{
			vld.isNew = true;
			vld.isLocked = false;
			serializeVoiceLines( true );
		}
		else
		{
			Debug.LogWarning("The voice line with id " + uniqueId + " that you want to unlock could not be found." );
		}
	}

	public int getEquippedVoiceLineIdForHero( string heroName )
	{
		VoiceLineData vld = voiceLineList.Find(voiceLineData => voiceLineData.heroName == heroName && voiceLineData.isEquipped == true );
		if( vld != null )
		{
			return vld.uniqueId;
		}
		else
		{
			Debug.LogError("The hero " + heroName + " does not have an equipped voice line." );
			return -1;
		}
	}

	public void unquippedVoiceLineForHero( string heroName )
	{
		VoiceLineData vld = voiceLineList.Find(voiceLineData => voiceLineData.heroName == heroName && voiceLineData.isEquipped == true );
		if( vld != null )
		{
			vld.isEquipped = false;
		}
		else
		{
			Debug.LogError("The hero " + heroName + " does not have an equipped voice line." );
		}
	}

	public void serializeVoiceLines( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setVoiceLines( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	[System.Serializable]
	public class VoiceLineData
	{
		public int uniqueId = 0; 
		public string heroName;
		public string clipName; //not strictly needed, but helps to debug when you can see the audio clip name
		public bool isNew = false;
		public bool isLocked = true;
		//You can only have one taunt equipped per hero
		public bool isEquipped = false;
	}

}
