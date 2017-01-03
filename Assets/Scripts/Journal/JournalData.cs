using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		public string entryName = string.Empty;
		public string coverName = string.Empty;
		public string storyName = string.Empty;
		public int numberOfPartsNeededToUnlock = 2;

		public JournalEntryStatus status = JournalEntryStatus.Locked;
		public int numberOfPartsDiscovered = 0;

		public JournalEntry( string entryName, string coverName, string storyName, int numberOfPartsNeededToUnlock )
		{
			this.entryName = entryName;
			this.coverName = coverName;
			this.storyName = storyName;
			this.numberOfPartsNeededToUnlock = numberOfPartsNeededToUnlock;
		}		

		public void printJournalEntry()
		{
			string printStr = entryName + " " + coverName + " " + storyName + " " + status + " " + numberOfPartsDiscovered + "/" + numberOfPartsNeededToUnlock;
			Debug.Log( "Journal Entry: " + printStr );
		}
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
			Debug.Log("JournalData-newPartAcquired for ID: " + activeUniqueId + " " + journalEntryList[ activeUniqueId ].entryName + " " + journalEntryList[ activeUniqueId ].numberOfPartsDiscovered + "/" + journalEntryList[ activeUniqueId ].numberOfPartsNeededToUnlock  );
			if(journalEntryUpdate != null) journalEntryUpdate( JournalEntryEvent.NewPartFound, journalEntryList[ activeUniqueId ] );
		}
		serializeJournalEntries();
	}

	public void serializeJournalEntries()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setJournalEntries( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}

}
