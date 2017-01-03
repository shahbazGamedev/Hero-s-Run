using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JournalEntryStatus {
	Locked = 0,
	Unlocked = 1
}

[System.Serializable]
public class JournalData {

	public List<JournalEntry> journalEntryList = new List<JournalEntry>();
	public int activeUniqueId = 0;

	[System.Serializable]
	public class JournalEntry
	{
		public string name = string.Empty;
		public JournalEntryStatus status = JournalEntryStatus.Locked;
		public int numberOfPartsNeededToUnlock = 2;
		public int numberOfPartsDiscovered = 0;

		public JournalEntry( string name, int numberOfPartsNeededToUnlock )
		{
			this.name = name;
			this.numberOfPartsNeededToUnlock = numberOfPartsNeededToUnlock;
		}		

		public void printJournalEntry()
		{
			string printStr = name + " " + status + " " + numberOfPartsDiscovered + "/" + numberOfPartsNeededToUnlock;
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

	public bool newPartAcquired()
	{
		//Did we already unlock the active journal entry?
		if( journalEntryList[ activeUniqueId ].status == JournalEntryStatus.Unlocked ) return false;

		bool wasEntryCompleted;
		journalEntryList[ activeUniqueId ].numberOfPartsDiscovered++;
		//Did we complete the set?
		if( journalEntryList[ activeUniqueId ].numberOfPartsDiscovered == journalEntryList[ activeUniqueId ].numberOfPartsNeededToUnlock )
		{
			Debug.Log("JournalData-newPartAcquired-new entry unlocked!");
			journalEntryList[ activeUniqueId ].status = JournalEntryStatus.Unlocked;
			//Do we have any more journal entries to unlock?
			if( activeUniqueId < journalEntryList.Count-1 ) activeUniqueId++;
			wasEntryCompleted = true;
		}
		else
		{
			Debug.Log("JournalData-newPartAcquired for " + activeUniqueId + " " + journalEntryList[ activeUniqueId ].numberOfPartsDiscovered + "/" + journalEntryList[ activeUniqueId ].numberOfPartsNeededToUnlock  );
			wasEntryCompleted = false;
		}
		serializeJournalEntries();
		return wasEntryCompleted;
	}

	public void serializeJournalEntries()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setJournalEntries( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}

}
