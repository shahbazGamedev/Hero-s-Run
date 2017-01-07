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
	public Text menuTitle;
	public Sprite lockIcon;
	public int lockIconSize = 110;
	

	// Use this for initialization
	void Awake () {
	
		menuTitle.text = LocalizationManager.Instance.getText("JOURNAL_CHRONICLES_OF_LUM");
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
			print( "Number of journal entries: " + journalData.getNumberOfVisibleEntries() + "/" + journalData.journalEntryList.Count );
			for( int i = 0; i < journalData.journalEntryList.Count; i++ )
			{
				//The hide bool is used to give more control on the server side on which stories are visible to the players.
				if( !journalData.journalEntryList[i].hide ) addEntry( journalData.journalEntryList[i] );
			}
		}
	}

	void addEntry( JournalData.JournalEntry journalEntry )
	{
		print("Adding " + journalEntry.title );
		GameObject go = (GameObject)Instantiate(entryPrefab);
		go.transform.SetParent(content);
		go.GetComponent<RectTransform>().localScale = new Vector3( 1f, 1f, 1f );
		JournalData.EntryMetadata entryMetadata = extractMetadata( GameManager.Instance.journalAssetManager.getStory( journalEntry.storyName ) );
		journalEntry.title = entryMetadata.title;
		Text[] entryTexts = go.GetComponentsInChildren<Text>();
		//Text 0 is the title
		entryTexts[0].text = journalEntry.title;
		Image[] entryImages = go.GetComponentsInChildren<Image>();
		//Image 0 is entry background.
		//Image 1 is the cover.
		if( journalEntry.status == JournalEntryStatus.Locked )
		{
			entryImages[1].sprite = lockIcon;
			entryImages[1].rectTransform.sizeDelta = new Vector2( lockIconSize, lockIconSize );
			//Text 1 is Story by
			entryTexts[1].enabled = false;
			//Text 2 is Cover Art by
			entryTexts[2].enabled = false;
		}
		else
		{
			entryImages[1].sprite = GameManager.Instance.journalAssetManager.getCover(journalEntry.coverName );
			//Text 1 is Story by
			string story_author = LocalizationManager.Instance.getText( "JOURNAL_STORY_BY" ).Replace( "<story_author>", entryMetadata.story_author );
			entryTexts[1].text = story_author;
			//Text 2 is Cover Art by
			string illustration_author = LocalizationManager.Instance.getText( "JOURNAL_COVER_ART_BY" ).Replace( "<illustration_author>", entryMetadata.illustration_author );
			entryTexts[2].text = illustration_author;
		}
		//Image 2 is the New icon. If the entry is New, it implicitly means it is unlocked.
		if( journalEntry.isNew )
		{
			entryImages[2].enabled = true;
		}
		else
		{
			entryImages[2].enabled = false;
		}

		Button[] entryButton = go.GetComponentsInChildren<Button>();
		Button button = entryButton[0];
		button.onClick.AddListener(() => entryButtonClick( journalEntry, entryImages[2] ));
	}

	JournalData.EntryMetadata extractMetadata( string text )
	{
		int indexOpeningBracket = text.IndexOf("{");
		int indexClosingBracket = text.LastIndexOf("}");
		if( indexOpeningBracket == -1 || indexOpeningBracket == -1 )
		{
			Debug.LogWarning("JournalMenu-extractMetadata: could not find metadata.");
			return null;
		}
		string json = text.Substring( indexOpeningBracket, indexClosingBracket + 1 );
		return JsonUtility.FromJson<JournalData.EntryMetadata>(json);
	}

	void entryButtonClick( JournalData.JournalEntry journalEntry, Image newIcon )
	{
		if( journalEntry.status == JournalEntryStatus.Unlocked )
		{
			if( UISoundManager.uiSoundManager != null ) UISoundManager.uiSoundManager.playButtonClick();
			createPages.generatePages( journalEntry );
			gameObject.SetActive( false );
			if( journalEntry.isNew )
			{
				if( journalData.journalEntryList.Contains( journalEntry ) )
				{
					journalData.journalEntryList.Find(x => x == journalEntry).isNew = false;
					newIcon.enabled = false;
					journalData.serializeJournalEntries();
				}
			}
		}
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
