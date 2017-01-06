using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum JournalEntryStatus {
	Locked = 0,
	Unlocked = 1
}

public enum JournalEntryEvent {
	EntryUnlocked = 0,
	NewPartFound = 1
}

[System.Serializable]
public class JournalData {

	public List<JournalEntry> journalEntryList = new List<JournalEntry>();
	public int activeUniqueId = 0;
	//Event management used to notify other classes when the character state has changed
	public delegate void JournalEntryUpdate( JournalEntryEvent journalEvent, JournalEntry journalEntry );
	public static event JournalEntryUpdate journalEntryUpdate;

	[System.Serializable]
	public class JournalEntry
	{
		//static
		public string title = string.Empty;
		public string coverName = string.Empty;
		public string storyName = string.Empty;
		public int numberOfPartsNeededToUnlock = 2;
		public string date_created = string.Empty;		//DateTime is not serializable. This value comes from entries.
		public DateTime dateTimecreated;				//The dateTimecreated value is created at runtime when the data is parsed. It is used for sorting the list.

		//Dynamic
		public JournalEntryStatus status = JournalEntryStatus.Locked;
		public int numberOfPartsDiscovered = 0;
		public bool isNew = false; 	//If isNew is true, it means the player has never viewed the story.
		public bool hide = false; 	//If hide is true, it means the entry will not be displayed in the journal list.

		public JournalEntry( string title, string coverName, string storyName, int numberOfPartsNeededToUnlock )
		{
			this.title = title;
			this.coverName = coverName;
			this.storyName = storyName;
			this.numberOfPartsNeededToUnlock = numberOfPartsNeededToUnlock;
		}		

		public void printJournalEntry()
		{
			string printStr = title + " " + coverName + " " + storyName + " " + status + " " + numberOfPartsDiscovered + "/" + numberOfPartsNeededToUnlock;
			Debug.Log( "Journal Entry: " + printStr );
		}
	}

	[System.Serializable]
	public class EntryMetadata
	{
		public string title = string.Empty;
		public string story_author = string.Empty;
		public string illustration_author = string.Empty;
	}

	public void addJournalEntry( JournalEntry journalEntry )
	{
		journalEntryList.Add(journalEntry);
	}

	public bool areAllEntriesUnlocked()
	{
		for( int i = 0; i < journalEntryList.Count; i++ )
		{
			if( journalEntryList[i].status == JournalEntryStatus.Locked ) return false;
		}
		return true;
	}

	public int getNumberOfNewEntries()
	{
		int newEntries = 0;
		for( int i = 0; i < journalEntryList.Count; i++ )
		{
			if( journalEntryList[i].status == JournalEntryStatus.Unlocked && journalEntryList[i].isNew ) newEntries++; 
		}
		return newEntries;
	}

	public void printAllEntries()
	{
		for( int i = 0; i < journalEntryList.Count; i++ )
		{
			journalEntryList[i].printJournalEntry();
		}
	}

	public void newPartAcquired()
	{
		//Did we already unlock the active journal entry?
		if( journalEntryList[ activeUniqueId ].status == JournalEntryStatus.Unlocked ) return;

		journalEntryList[ activeUniqueId ].numberOfPartsDiscovered++;
		//Did we complete the set?
		if( journalEntryList[ activeUniqueId ].numberOfPartsDiscovered == journalEntryList[ activeUniqueId ].numberOfPartsNeededToUnlock )
		{
			Debug.Log("JournalData-newPartAcquired-new entry unlocked!");
			journalEntryList[ activeUniqueId ].status = JournalEntryStatus.Unlocked;
			journalEntryList[ activeUniqueId ].isNew = true;
			if(journalEntryUpdate != null) journalEntryUpdate( JournalEntryEvent.EntryUnlocked, journalEntryList[ activeUniqueId ] );
			//Do we have any more journal entries to unlock?
			if( activeUniqueId < journalEntryList.Count-1 ) activeUniqueId++;
			//If the player finds the last part of the last entry (and therefore he has unlocked all of the Journal entries),
			//We need to destroy any remaining story unlock powerups in the level, because they will be useless for the player.
			if( areAllEntriesUnlocked() )
			{
				//Reminder that GameObject.FindGameObjectsWithTag does not find inactive game objects
				PowerUp[] storyUnlocksArray = Resources.FindObjectsOfTypeAll<PowerUp>();
				Debug.Log("JournalData-newPartAcquired-everything is now unlocked. Make remaining Story Unlock powerups inactive: " + storyUnlocksArray.Length );
				for( int i = 0; i < storyUnlocksArray.Length; i++ )
				{
					if( storyUnlocksArray[i].powerUpType == PowerUpType.StoryUnlock ) storyUnlocksArray[i].gameObject.SetActive( false );
					//Important: This will also make the prefab inactive. So, make sure to set the PowerUp active when adding it to the level.
				}
			}
			
		}
		else
		{
			Debug.Log("JournalData-newPartAcquired for ID: " + activeUniqueId + " " + journalEntryList[ activeUniqueId ].title + " " + journalEntryList[ activeUniqueId ].numberOfPartsDiscovered + "/" + journalEntryList[ activeUniqueId ].numberOfPartsNeededToUnlock  );
			if(journalEntryUpdate != null) journalEntryUpdate( JournalEntryEvent.NewPartFound, journalEntryList[ activeUniqueId ] );
		}
		serializeJournalEntries();
	}

	public void convertStringDates()
	{
		for( int i = 0; i < journalEntryList.Count; i++ )
		{
			try
			{
				journalEntryList[i].dateTimecreated = DateTime.Parse( journalEntryList[i].date_created );
			}
			catch( Exception e )
			{
				Debug.LogWarning("JournalData-convertStringDates: unable to parse date : " + journalEntryList[i].date_created + ". Error is: " + e.Message );
			}
		}
		//Now order the list from most recent entry to oldest
		journalEntryList.Sort((x, y) => -x.dateTimecreated.CompareTo(y.dateTimecreated));
	}

	public void serializeJournalEntries()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setJournalEntries( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}

}
