using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class LootBoxUnlockNowPopup : MonoBehaviour {

	[Header("Loot box popup")]
	[SerializeField] TextMeshProUGUI titleText;
	[SerializeField] TextMeshProUGUI earnedAtBaseText;

	[Header("Credit range")]
	[SerializeField] TextMeshProUGUI creditsRangeText;

	[Header("Number of cards")]
	[SerializeField] TextMeshProUGUI numberOfCardsText;

	[Header("Contains at least (only used for gold chests)")]
	[SerializeField] GameObject containsAtLeast;
	[SerializeField] TextMeshProUGUI numberOfSpecialCardsText;
	[SerializeField] TextMeshProUGUI rarityOfSpecialCardsText;

	[Header("Unlock")]
	[SerializeField] GameObject unlock;
	[SerializeField] TextMeshProUGUI unlockTimeText;

	[Header("Unlocking")]
	[SerializeField] GameObject unlocking;
	[SerializeField] TextMeshProUGUI timeRemainingText;
	[SerializeField] TextMeshProUGUI openNowCostText;

	// Use this for initialization
	public void configure(  LootBoxData lootBoxData, LootBoxOwnedData lootBoxOwnedData )
	{

		if( lootBoxOwnedData.state == LootBoxState.UNLOCKING ) StartCoroutine( updateTimeRemaining( lootBoxOwnedData.getUnlockStartTimeTime().AddHours( lootBoxData.timeToUnlockInHours ) ) );

		//Loot box popup
		//Localize the loot box name
		titleText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_" + lootBoxData.type.ToString().ToUpper() );
		//Earned at base
		string earnedForString = string.Format( LocalizationManager.Instance.getText( "LOOTBOX_EARNED_FOR_UNLOCKING_BASE" ), lootBoxOwnedData.earnedInBase );
		earnedAtBaseText.text = earnedForString;
	
		//Credit range
		creditsRangeText.text = "";
	
		//Number of cards
		string xString = LocalizationManager.Instance.getText( "LOOT_BOX_POPUP_X" );
		numberOfCardsText.text = string.Format( xString, lootBoxData.containsThisAmountOfCards );
	
		//Contains at least (only used for gold chests)
		containsAtLeast.SetActive( false );
		numberOfSpecialCardsText.text = string.Format( xString, lootBoxData.containsAtLeastThisAmountOfRareCards );
		rarityOfSpecialCardsText.text = "Rare";
	
		//Unlock
		unlock.SetActive( lootBoxOwnedData.state == LootBoxState.READY_TO_UNLOCK );
		unlockTimeText.text = lootBoxData.timeToUnlockInHours.ToString();
	
		//Unlocking
		unlocking.SetActive( lootBoxOwnedData.state == LootBoxState.UNLOCKING );
		timeRemainingText.text = "";
		openNowCostText.text = lootBoxData.unlockHardCurrencyCost.ToString();
		
	}

	IEnumerator updateTimeRemaining( DateTime openLootBoxTime )
	{
		while( DateTime.UtcNow < openLootBoxTime )
		{
			TimeSpan openTime = openLootBoxTime.Subtract( DateTime.UtcNow );
			string timeDisplayed = string.Format( LocalizationManager.Instance.getText( "LOOT_BOX_TIME_FORMAT" ), openTime.Hours, openTime.Minutes, openTime.Seconds );
			timeRemainingText.text = timeDisplayed;
			//Update every fifteen seconds
			yield return new WaitForSecondsRealtime( 15 );
		}
		//The loot box is ready to open
		//timeRemaining.SetActive (false);

	}

	public void OnClickHide()
	{
		StopAllCoroutines();
		gameObject.SetActive( false );
	}
	
}
