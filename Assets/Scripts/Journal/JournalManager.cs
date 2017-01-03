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
			JournalData.JournalEntry entry = new JournalData.JournalEntry( "The Treasure", 3 );
			journalData.addJournalEntry( entry );
			entry = new JournalData.JournalEntry( "The Secret Passage", 4 );
			journalData.addJournalEntry( entry );
			journalData.serializeJournalEntries();
		}
		GameManager.Instance.journalData = journalData;
	}
	
}
