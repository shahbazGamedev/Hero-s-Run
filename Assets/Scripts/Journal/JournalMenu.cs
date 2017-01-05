using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JournalMenu : MonoBehaviour {

	bool levelLoading = false;
	JournalData journalData;
	public RectTransform content;
	public GameObject entryPrefab;
	public CreatePages createPages;
	

	// Use this for initialization
	void Start ()
	{
		generateEntries();
	}

	public void generateEntries()
	{
		JournalManager journalManager = GameObject.FindObjectOfType<JournalManager>();
		if( journalManager != null )
		{
			journalData = journalManager.journalData;
		}
		else
		{
			//Create a dummy journal data here just so we can test directly in the scene
			journalData = new JournalData();
			//Add some entries
			JournalData.JournalEntry entry = new JournalData.JournalEntry( "The Treasure", "Cover 1", "Story 1", 3 );
			journalData.addJournalEntry( entry );
			entry = new JournalData.JournalEntry( "The Secret Passage", "Cover 2", "Story 2", 4 );
			journalData.addJournalEntry( entry );
		}

		print( "Number of journal entries " +  journalData.journalEntryList.Count );
		for( int i = 0; i < journalData.journalEntryList.Count; i++ )
		{
			addEntry( journalData.journalEntryList[i] );
		}
	}

	void addEntry( JournalData.JournalEntry journalEntry )
	{
		print("Adding " + journalEntry.title );
		GameObject go = (GameObject)Instantiate(entryPrefab);
		go.transform.SetParent(content);
		Button[] entryButton = go.GetComponentsInChildren<Button>();
		Button button = entryButton[0];
		button.onClick.AddListener(() => entryButtonClick(journalEntry));
		Text[] entryTexts = go.GetComponentsInChildren<Text>();
		entryTexts[0].text = journalEntry.title;
		//entryTexts[1].text = journalEntry.title;
		//entryTexts[2].text = journalEntry.title;
	}

	void entryButtonClick( JournalData.JournalEntry journalEntry )
	{
		Debug.Log("entryButtonClick: " + journalEntry.title );
		createPages.generatePages( journalEntry );
		gameObject.SetActive( false );
		if( UISoundManager.uiSoundManager != null ) UISoundManager.uiSoundManager.playButtonClick();
	}

	public void OnClickCloseMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			GameManager.Instance.setGameState(GameState.WorldMapNoPopup);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
