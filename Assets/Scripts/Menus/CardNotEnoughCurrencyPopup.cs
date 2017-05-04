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

	public void configure ( int gemsNeeded )
	{
		titleText.text = "Not Enough Coins!";
		convertButtonText.text = gemsNeeded.ToString();
		descriptionText.text = "Buy missing coins?";
		convertButton.gameObject.SetActive( true );
		convertButton.onClick.RemoveAllListeners();
		convertButton.onClick.AddListener(() => OnClickConvert( gemsNeeded ) );
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

	public void OnClickConvert( int gemsNeeded )
	{
		print("OnClickConvert");
		UISoundManager.uiSoundManager.playButtonClick();
		//Does the player have enough gems?
		if( gemsNeeded <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			//Yes, he does
			GameManager.Instance.playerInventory.deductGems( gemsNeeded );
			print("deductGems " + gemsNeeded);
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
