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
	void Awake () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif
	}

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
			entry.isNew = true;
			entry.status = JournalEntryStatus.Unlocked;
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
		//Text 0 is the title
		entryTexts[0].text = journalEntry.title;
		//Text 1 is Story by
		entryTexts[1].text = LocalizationManager.Instance.getText( "JOURNAL_STORY_BY" ) + " ";
		//Text 2 is Cover Art by
		entryTexts[2].text = LocalizationManager.Instance.getText( "JOURNAL_COVER_ART_BY" ) + " ";
		//Text 3 is the NEW indicator (which means the player has never opened the unlocked story).
		if( journalEntry.isNew )
		{
			entryTexts[3].text = LocalizationManager.Instance.getText( "JOURNAL_NEW" );
			entryTexts[3].enabled = true;
		}
		else
		{
			entryTexts[3].enabled = false;
		}
		Image[] entryImages = go.GetComponentsInChildren<Image>();
		//Image 0 is entry background.
		//Image 1 is the lock icon on the right hand side.
		if( journalEntry.status == JournalEntryStatus.Locked )
		{
			entryImages[1].enabled = true;
		}
		else
		{
			entryImages[1].enabled = false;
		}
		//Image 2 is the icon on the left hand side. This never changes.
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
