using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalHUD : MonoBehaviour {

	public Image cover;
	public Text numberOfParts;
	public Sprite coverWhenIncomplete;

	// Use this for initialization
	void Awake ()
 	{
		cover.gameObject.SetActive( false );
		numberOfParts.gameObject.SetActive( false );
	}

	void fadeInJournal()
	{
		cover.color = new Color( cover.color.r, cover.color.g, cover.color.b, 0f );
		numberOfParts.color = new Color( numberOfParts.color.r, numberOfParts.color.g, numberOfParts.color.b, 0f );
		cover.rectTransform.localScale = Vector3.zero;
		cover.gameObject.SetActive( true );
		numberOfParts.gameObject.SetActive( true );
		LeanTween.color( cover.rectTransform, new Color( cover.color.r, cover.color.g, cover.color.b, 1f ), 0.6f ).setOnComplete(fadeOutJournal).setOnCompleteParam(gameObject);
		LeanTween.alphaText( numberOfParts.rectTransform, 1f, 0.6f );
		LeanTween.scale ( cover.rectTransform, Vector3.one, 0.6f ).setEase(LeanTweenType.easeOutQuad);
	}

	void fadeOutJournal()
	{
		LeanTween.color( cover.rectTransform, new Color( cover.color.r, cover.color.g, cover.color.b, 0 ), 0.6f ).setOnComplete(hideJournal).setOnCompleteParam(gameObject).setDelay(3.5f);
		LeanTween.alphaText( numberOfParts.rectTransform, 0, 0.6f ).setDelay(3.4f);
		LeanTween.scale ( cover.rectTransform, Vector3.zero, 0.6f ).setEase(LeanTweenType.easeOutQuad).setDelay(3.5f);
	}

	void hideJournal()
	{
		cover.gameObject.SetActive( false );
		numberOfParts.gameObject.SetActive( false );
	}

	void OnEnable()
	{
		JournalData.journalEntryUpdate += JournalEntryUpdate;
	}
	
	void OnDisable()
	{
		JournalData.journalEntryUpdate -= JournalEntryUpdate;
	}

	void JournalEntryUpdate( JournalEntryEvent journalEntryEvent, JournalData.JournalEntry journalEntry )
	{
		Debug.Log("JournalHUD-JournalEntryUnlocked " + journalEntryEvent.ToString() + " " + journalEntry.entryName );
		if( journalEntryEvent == JournalEntryEvent.EntryUnlocked )
		{
			cover.sprite = GameManager.Instance.journalAssetManager.covers[journalEntry.coverName];
			numberOfParts.text = LocalizationManager.Instance.getText("JOURNAL_UNLOCKED");
			fadeInJournal();
		}
		else if( journalEntryEvent == JournalEntryEvent.NewPartFound )
		{
			cover.sprite = coverWhenIncomplete;
			numberOfParts.text = journalEntry.numberOfPartsDiscovered.ToString() + "/" + journalEntry.numberOfPartsNeededToUnlock.ToString();
			fadeInJournal();
		}	
	}
	
}
