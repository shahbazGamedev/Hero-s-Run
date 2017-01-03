using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalManager : MonoBehaviour {

	public JournalData journalData;

	// Use this for initialization
	void Awake ()
	{
		if( PlayerStatsManager.Instance.getJournalEntries() != string.Empty )
		{
			 journalData = JsonUtility.FromJson<JournalData>(PlayerStatsManager.Instance.getJournalEntries());
		}
		else
		{
			journalData = new JournalData();
			//Add some entries
			JournalData.JournalEntry entry = new JournalData.JournalEntry( "The Treasure", "Cover 1", "Story 1", 3 );
			journalData.addJournalEntry( entry );
			entry = new JournalData.JournalEntry( "The Secret Passage", "Cover 2", "Story 2", 4 );
			journalData.addJournalEntry( entry );
			journalData.serializeJournalEntries();
		}
		GameManager.Instance.journalData = journalData;

	}

	//Called by JournalAssetManager when entries received from cache or server
	public void updateEntries( string entriesFromServer )
	{
		JsonUtility.FromJsonOverwrite( entriesFromServer, journalData );
	} 
	
}
