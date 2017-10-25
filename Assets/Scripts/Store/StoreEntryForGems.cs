using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class StoreEntryForGems : MonoBehaviour {

	[Header("Store Entry For Gems")]
	[SerializeField]  Text title;
	[SerializeField]  string titleID;
	[Tooltip("For example, the number of gems you get with your purchase. Gems are bought with real money.")]
	[SerializeField]  Text quantityText;
	[SerializeField]  int quantity;
	[SerializeField]  Text buyGemsButtonLabel;
	[Tooltip("Gems price will ultimately come from the app store. For now, we use dummy values.")]
	[SerializeField]  float price;

	string currencySymbol = "$";

	// Use this for initialization
	void Awake ()
	{
		title.text = LocalizationManager.Instance.getText(titleID);
		buyGemsButtonLabel.text = currencySymbol + price.ToString();
		quantityText.text = quantity.ToString("N0");
	}

	public void buy()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//We need Internet to make a real money Gem purchase
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			//Player is connected to the Internet
			Debug.Log("Buy " + quantity + " gems.");
			GameManager.Instance.playerInventory.addGems( quantity );
		}
		else
		{
			//Player is not connected to the Internet
			MultiPurposePopup.Instance.displayPopup( "MENU_NO_INTERNET" );
		}
	}
}
