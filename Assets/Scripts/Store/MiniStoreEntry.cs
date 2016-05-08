using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MiniStoreEntry : MonoBehaviour {

	[Header("Mini Store Entry")]
	public Text description;
	public int quantity; 	//For example number of lives you are buying
	public float price; 	//For testing. real prices will come from app store
	public Button buyButton;
	public Text buyButtonLabel;
	string currencySymbol = "$";

	// Use this for initialization
	void Awake () {

		initializePurchaseLivesEntry();
	}
	
	void initializePurchaseLivesEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("STORE_ITEM_LIVES_DESCRIPTION");
		//Replace the string <quantity> by the quantity the player will receive if he makes the purchase
		descriptionString = descriptionString.Replace( "<quantity>", quantity.ToString("N0") );
		description.text = descriptionString;
		
		buyButtonLabel.text = currencySymbol + price.ToString();
	}
	
	void buyLives()
	{
		Debug.Log("buyLives");
		SoundManager.playButtonClick();

		//Grant the purchased lives
		PlayerStatsManager.Instance.increaseLives( quantity );

		//Save the data
		PlayerStatsManager.Instance.savePlayerStats();
	}

}
