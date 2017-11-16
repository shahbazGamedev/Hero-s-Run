using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class LootBoxUnlockNowPopup : MonoBehaviour {

	[Header("Loot box popup")]
	[SerializeField] LootBoxCanvas lootBoxCanvas;
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

	private LootBoxOwnedData lootBoxOwnedData;
	private LootBoxData lootBoxData;

	// Use this for initialization
	public void configure(  LootBoxData lootBoxData, LootBoxOwnedData lootBoxOwnedData )
	{
		//Keep a copy
		this.lootBoxOwnedData = lootBoxOwnedData;
		this.lootBoxData = lootBoxData;

		//Loot box popup
		//Localize the loot box name
		titleText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_" + lootBoxData.type.ToString().ToUpper() );
		//Earned at base
		string earnedForString = string.Format( LocalizationManager.Instance.getText( "LOOTBOX_BASE" ), lootBoxOwnedData.earnedInBase );
		earnedAtBaseText.text = earnedForString;
	
		//Credit range
		string rangeString = string.Format( LocalizationManager.Instance.getText( "LOOTBOX_SOFT_CURRENCY_RANGE" ), lootBoxData.containsThisRangeOfSoftCurrency.x, lootBoxData.containsThisRangeOfSoftCurrency.y );
		creditsRangeText.text = rangeString;
	
		//Number of cards
		string xString = LocalizationManager.Instance.getText( "LOOT_BOX_X" );
		numberOfCardsText.text = string.Format( xString, lootBoxData.containsThisAmountOfCards );
	
		//Contains at least (only used for gold loot boxes)
		//Not implemented for now
		containsAtLeast.SetActive( false );
		numberOfSpecialCardsText.text = string.Format( xString, lootBoxData.containsAtLeastThisAmountOfRareCards );
	
		//Note: you will never be in the LootBoxState.UNLOCKED state if this popup is displayed.
		if( lootBoxOwnedData.state == LootBoxState.READY_TO_UNLOCK )
		{
			//Unlock
			unlock.SetActive( true );
			unlocking.SetActive( false );
			
			string hourString = LocalizationManager.Instance.getText( "LOOT_BOX_HOUR" );
			unlockTimeText.text = string.Format( hourString, lootBoxData.timeToUnlockInHours );
		}
		else if ( lootBoxOwnedData.state == LootBoxState.UNLOCKING )
		{
			//Unlocking
			unlock.SetActive( false );
			unlocking.SetActive( true );
			StartCoroutine( updateValues( lootBoxOwnedData.getUnlockStartTimeTime().AddHours( lootBoxData.timeToUnlockInHours ) ) );
			//If the player doesn't have enough hard currency, make the cost text color red.
			if( GameManager.Instance.playerInventory.getGemBalance() >= lootBoxData.unlockHardCurrencyCost )
			{
				openNowCostText.color = Color.white;
			}
			else
			{
				openNowCostText.color = Color.red;
			}
			openNowCostText.text = lootBoxData.unlockHardCurrencyCost.ToString();
		}
		
	}

	//Update both the time remaining and the cost.
	//The cost to open now decreases linearly with time.
	IEnumerator updateValues( DateTime openLootBoxTime )
	{
		//Cost related variables
		float updatedCost = 0;
		int startCost = lootBoxData.unlockHardCurrencyCost;
		float timeToUnlockInSeconds = lootBoxData.timeToUnlockInHours * 3600; //convert to seconds

		while( DateTime.UtcNow < openLootBoxTime )
		{
			TimeSpan timeLeft = openLootBoxTime.Subtract( DateTime.UtcNow );

			//Update the time remaining
			string timeDisplayed = string.Format( LocalizationManager.Instance.getText( "LOOT_BOX_TIME_FORMAT" ), timeLeft.Hours, timeLeft.Minutes );
			timeRemainingText.text = timeDisplayed;

			//Update the cost
			float percentage = (float)timeLeft.TotalSeconds/timeToUnlockInSeconds;
			updatedCost = percentage * startCost;
			openNowCostText.text = updatedCost.ToString("F0");

			//If the player doesn't have enough hard currency, make the cost text color red.
			if( GameManager.Instance.playerInventory.getGemBalance() >= updatedCost )
			{
				openNowCostText.color = Color.white;
			}
			else
			{
				openNowCostText.color = Color.red;
			}

			//Update every second
			yield return new WaitForSecondsRealtime( 1 );
		}
		//The loot box is unlocked and ready to be opened.
		//The popup is no longer needed. OnClickHide will make sure the loot box configuration is updated.
		lootBoxOwnedData.state = LootBoxState.UNLOCKED;
		GameManager.Instance.playerInventory.serializePlayerInventory( true );
		OnClickHide();

	}

	public void OnClickStartUnlock()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		lootBoxOwnedData.setUnlockStartTime( DateTime.UtcNow );
		lootBoxOwnedData.state = LootBoxState.UNLOCKING;
		GameManager.Instance.playerInventory.serializePlayerInventory( true );
		lootBoxCanvas.updateRaceWonData();
		OnClickHide();
	}

	public void OnClickOpenNow()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//Make sure the player has enough hard currency to unlock immediately
		if( GameManager.Instance.playerInventory.getGemBalance() >= lootBoxData.unlockHardCurrencyCost )
		{
			GameManager.Instance.playerInventory.deductGems( lootBoxData.unlockHardCurrencyCost );
			lootBoxOwnedData.state = LootBoxState.UNLOCKED;
			GameManager.Instance.playerInventory.serializePlayerInventory( true );
			StopAllCoroutines();
			gameObject.SetActive( false );
			//Show the canvas
			lootBoxCanvas.gameObject.SetActive( true );
			lootBoxCanvas.OpenRaceWonLootBoxImmediately();
		}
		else
		{
			//To do
			//Show the not enough hard currency popup.
		}
	}

	public void OnClickHide()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StopAllCoroutines();
		gameObject.SetActive( false );
		//Show the canvas
		lootBoxCanvas.gameObject.SetActive( true );
	}
	
}
