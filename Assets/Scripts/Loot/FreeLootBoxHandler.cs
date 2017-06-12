using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class FreeLootBoxHandler : MonoBehaviour {

	[SerializeField] TextMeshProUGUI nextFreeLootBoxText;
	[SerializeField] TextMeshProUGUI freeLootBoxExplanationText;
	[SerializeField] GameObject nextOneText;
	const int HOURS_BETWEEN_FREE_LOOT_BOX = 4;

	void Start ()
	{
		if( DateTime.UtcNow > getOpenTime() )
		{
			nextOneText.SetActive( false );
			nextFreeLootBoxText.text = LocalizationManager.Instance.getText("FREE_LOOT_BOX_OPEN");
		}
		else
		{
			StartCoroutine( updateNextFreeLootBoxTime() );
		}
	}

	IEnumerator updateNextFreeLootBoxTime()
	{
		nextOneText.SetActive( true );
		while( DateTime.UtcNow < getOpenTime() )
		{
			TimeSpan openTime = getOpenTime().Subtract( DateTime.UtcNow );
			string timeDisplayed = string.Format( LocalizationManager.Instance.getText( "FREE_LOOT_BOX_TIME_FORMAT" ), openTime.Hours, openTime.Minutes, openTime.Seconds );
			nextFreeLootBoxText.text = timeDisplayed;
			//Update every fifteen seconds
			yield return new WaitForSecondsRealtime( 15 );
		}
		nextOneText.SetActive( false );
		nextFreeLootBoxText.text = LocalizationManager.Instance.getText("FREE_LOOT_BOX_OPEN");
	}

	public void OnClickOpenFreeLootBox()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( DateTime.UtcNow > getOpenTime() )
		{
			//The free loot box is ready.
			LootBoxClientManager.Instance.requestLootBox( LootBoxType.FREE );
			GameManager.Instance.playerProfile.setLastFreeLootBoxOpenedTime( DateTime.UtcNow );
			//Schedule a local notification to remind the player of when his next free loot box will be available
			NotificationServicesHandler.Instance.scheduleFreeLootBoxNotification( HOURS_BETWEEN_FREE_LOOT_BOX * 60 );
			StartCoroutine( updateNextFreeLootBoxTime() );
		}
		else
		{
			CancelInvoke( "hideFreeLootBoxExplanationText" );
			//The free loot box is not ready. Display a message.
			freeLootBoxExplanationText.text = string.Format( LocalizationManager.Instance.getText("FREE_LOOT_BOX_EXPLANATION"), HOURS_BETWEEN_FREE_LOOT_BOX );
			freeLootBoxExplanationText.gameObject.SetActive( true );
			Invoke("hideFreeLootBoxExplanationText", 5f);
		}
	}

	DateTime getOpenTime()
	{
		return GameManager.Instance.playerProfile.getLastFreeLootBoxOpenedTime().AddHours(HOURS_BETWEEN_FREE_LOOT_BOX);
	}

	void hideFreeLootBoxExplanationText()
	{
		freeLootBoxExplanationText.gameObject.SetActive( false );
	}
	
}
