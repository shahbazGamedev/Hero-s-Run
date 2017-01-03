using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalHUD : MonoBehaviour {

	public Image cover;
	public Text numberOfParts;
	CanvasGroup canvasGroup;

	// Use this for initialization
	void Awake ()
 	{
		canvasGroup = this.GetComponent<CanvasGroup>();
		cover.gameObject.SetActive( false );
		numberOfParts.gameObject.SetActive( false );
	}

	void fadeInCover()
	{
		cover.color = new Color( cover.color.r, cover.color.g, cover.color.b, 0f );
		cover.rectTransform.localScale = Vector3.zero;
		cover.gameObject.SetActive( true );
		LeanTween.color( cover.rectTransform, new Color( cover.color.r, cover.color.g, cover.color.b, 1f ), 0.6f ).setOnComplete(fadeOutCover).setOnCompleteParam(gameObject);
		LeanTween.scale ( cover.rectTransform, Vector3.one, 0.6f ).setEase(LeanTweenType.easeOutQuad);
	}

	void fadeOutCover()
	{
		LeanTween.color( cover.rectTransform, new Color( cover.color.r, cover.color.g, cover.color.b, 0 ), 0.6f ).setOnComplete(hideCover).setOnCompleteParam(gameObject).setDelay(3.5f);
		LeanTween.scale ( cover.rectTransform, Vector3.zero, 0.6f ).setEase(LeanTweenType.easeOutQuad).setDelay(3.5f);
	}

	void hideCover()
	{
		cover.gameObject.SetActive( false );
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
		Debug.Log("JournalHUD-JournalEntryUnlocked " + journalEntryEvent.ToString() + " " + journalEntry.name );
		if( journalEntryEvent == JournalEntryEvent.EntryUnlocked )
		{
			fadeInCover();
			numberOfParts.gameObject.SetActive( false );
		}
		else if( journalEntryEvent == JournalEntryEvent.NewPartFound )
		{
			numberOfParts.gameObject.SetActive( true );
			numberOfParts.text = journalEntry.numberOfPartsDiscovered.ToString() + "/" + journalEntry.numberOfPartsNeededToUnlock.ToString();
		}	
	}
	
}
