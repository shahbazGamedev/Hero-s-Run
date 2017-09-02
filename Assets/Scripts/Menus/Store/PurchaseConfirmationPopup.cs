using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseConfirmationPopup : MonoBehaviour {

	[SerializeField] Text titleText;
	[SerializeField] Text descriptionText;
	[SerializeField] Button confirmButton;
	[SerializeField] Text confirmButtonText;
	[SerializeField] ScrollRect horizontalScrollview;

	public void Show( int priceInHardCurrency, string packageName, int softCurrencyQuantity )
	{
		//Disable scrolling while popup is displayed
		horizontalScrollview.enabled = false;
		confirmButton.onClick.RemoveAllListeners();
		confirmButton.onClick.AddListener(() => OnClickPurchase( priceInHardCurrency, softCurrencyQuantity ));
		gameObject.SetActive( true );
		confirmButtonText.text = priceInHardCurrency.ToString("N0");
		//Format is: "Buy {0} and get {1} credits."
		string description = LocalizationManager.Instance.getText( "PURCHASE_CONFIRMATION_POPUP_DESCRIPTION" );
		descriptionText.text = string.Format( description, packageName, softCurrencyQuantity.ToString("N0") );
 	}

	public void OnClickPurchase( int priceInHardCurrency, int softCurrencyQuantity )
	{
		Debug.Log("OnClickPurchase - buying " + softCurrencyQuantity + " credits with " + priceInHardCurrency + " Titanium." );
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		GameManager.Instance.playerInventory.deductGems( priceInHardCurrency );
		GameManager.Instance.playerInventory.addCoins( softCurrencyQuantity );
		//Re-enable scrolling
		horizontalScrollview.enabled = true;
	}

	public void OnClickHide()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		//Re-enable scrolling
		horizontalScrollview.enabled = true;
	}

}