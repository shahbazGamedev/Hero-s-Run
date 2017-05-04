using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardNotEnoughCurrencyPopup : MonoBehaviour {

	[Header("Description")]
	[SerializeField] Text titleText;
	[SerializeField] Text descriptionText;
	[SerializeField] Button convertButton;
	[SerializeField] Text convertButtonText;
	[SerializeField] Button goToShopButton;
	[SerializeField] Text goToShopButtonText;

	public void configure ( GameObject go, CardName card, int coinsAvailable, int gemsNeeded )
	{
		titleText.text = "Not Enough Coins!";
		convertButtonText.text = gemsNeeded.ToString();
		descriptionText.text = "Buy missing coins?";
		convertButton.gameObject.SetActive( true );
		convertButton.onClick.RemoveAllListeners();
		convertButton.onClick.AddListener(() => OnClickConvert( go, card, coinsAvailable, gemsNeeded ) );
		goToShopButton.gameObject.SetActive( false );
		//Does the player have enough gems?
		if( gemsNeeded <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			convertButtonText.color = Color.white;
		}
		else
		{
			convertButtonText.color = Color.red;
		}
	}

	public void OnClickConvert( GameObject go, CardName card, int coinsAvailable, int gemsNeeded )
	{
		print("OnClickConvert");
		UISoundManager.uiSoundManager.playButtonClick();
		//Does the player have enough gems?
		if( gemsNeeded <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			//Yes, he does
			PlayerStatsManager.Instance.deductCoins( coinsAvailable );
			GameManager.Instance.playerInventory.deductGems( gemsNeeded );
			GameManager.Instance.playerDeck.upgradeCardByOneLevel( card );
			//TO DO
			//Display upgrade ceremony panel
			//Update the Card Detail popup since some values have changed
			transform.parent.GetComponent<CardDetailPopup>().configureCard( go, GameManager.Instance.playerDeck.getCardByName( card ), CardManager.Instance.getCardByName( card ) );
			//Update the Card Collection entry as well
			go.GetComponent<CardUIDetails>().configureCard( GameManager.Instance.playerDeck.getCardByName( card ), CardManager.Instance.getCardByName( card ) );

			//Hide this popup
			gameObject.SetActive( false );
			print("Card upgraded " + card + " gemsNeeded " + gemsNeeded + " coinsAvailable " + coinsAvailable );
		}
		else
		{
			//No, he doesn't
		}
	}

	public void OnClickGoToShop()
	{
		print("OnClickGoToShop");
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		transform.parent.GetComponent<CardDetailPopup>().OnClickHide();
	}

	public void OnClickHide()
	{
		print("OnClickHide");
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
	}

}
