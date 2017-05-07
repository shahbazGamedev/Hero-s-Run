using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardNotEnoughCurrencyPopup : MonoBehaviour {

	[SerializeField] Text titleText;
	[SerializeField] Text descriptionText;
	[SerializeField] Button convertButton;
	[SerializeField] Text convertButtonText;
	[SerializeField] Button goToShopButton;
	[SerializeField] Text goToShopButtonText;

	public void configureForNotEnoughCoins ( GameObject go, CardName card, int coinsAvailable, int gemsNeeded )
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

	void OnClickConvert( GameObject go, CardName card, int coinsAvailable, int gemsNeeded )
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//Does the player have enough gems?
		if( gemsNeeded <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			//Yes, he does
			GameManager.Instance.playerInventory.deductCoins( coinsAvailable );
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
		}
		else
		{
			//No, he doesn't
			configureForNotEnoughGems();
		}
	}

	void configureForNotEnoughGems()
	{
		titleText.text = "Not Enough Gems!";
		descriptionText.text = "You don't have enough gems. No worries. You can get some at the shop.";
		convertButton.gameObject.SetActive( false );
		goToShopButton.gameObject.SetActive( true );
	}

	public void OnClickGoToShop()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		transform.parent.GetComponent<CardDetailPopup>().OnClickHide();
		UniversalTopBar.Instance.OnClickShowGemStore();
	}

	public void OnClickHide()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
	}

}
