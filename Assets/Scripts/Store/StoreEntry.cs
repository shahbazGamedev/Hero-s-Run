using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum PurchaseType {
	
	None = 0,
	Upgrade = 1,
	Consumable = 2,
	Purchase_Coin_Doubler = 3,
	Purchase_Coins = 4,
	Purchase_Lives = 5,
	Restore_Coin_Doubler = 6,
	Purchase_Gems = 7
}

public class StoreEntry : MonoBehaviour {

	[Header("Store Entry")]
	public PowerUpType powerUpType = PowerUpType.None;
	public PurchaseType purchaseType = PurchaseType.None;

	public Text title;
	public Text description;
	public int quantity; 	//For example number of coins you get with you purchase or number of lives
	public float price; 	//For testing. real prices will come from app store
	public Button buyButton;
	public Text buyButtonLabel;
	public Slider upgradeLevel;
	public string titleID = "POWER_UP_MAGNET";
	public string descriptionID = "POWER_UP_MAGNET_DESCRIPTION";
	//Valid upgrade values are integers between 0 to 6 included
	const int MAXIMUM_UPGRADE_LEVEL = 6;
	string currencySymbol = "$";

	// Use this for initialization
	void Awake () {

		//So I can see the text displayed without going through the load menu as well as have valid save data while in the editor
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); 
		PlayerStatsManager.Instance.loadPlayerStats();
		#endif

		if( title != null )title.text = LocalizationManager.Instance.getText(titleID);

		switch (purchaseType)
		{
	        case PurchaseType.Upgrade:
				initializeUpgradeEntry();
                break;
	                
	        case PurchaseType.Consumable:
				initializeConsumableEntry();
                break;
	                
	        case PurchaseType.Purchase_Coin_Doubler:
				initializeCoinDoublerEntry();
                break;
	                
	        case PurchaseType.Purchase_Lives:
				initializePurchaseLivesEntry();
                break;
	
	        case PurchaseType.Purchase_Coins:
				initializePurchaseCoinsEntry();
                break;

	        case PurchaseType.Purchase_Gems:
				initializePurchaseGemsEntry();
                break;

			 case PurchaseType.Restore_Coin_Doubler:
				initializeRestoreCoinDoublerEntry();
                break;

			 case PurchaseType.None:
				Debug.LogError( title.text + " The PurchaseType parameter has not been set. A value of NONE is not permitted.");
                break;
		}
	
	}

	void initializeUpgradeEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText(descriptionID);
		//Replace the string <time> by the time in seconds gained by the upgrade
		descriptionString = descriptionString.Replace( "<time>", PowerUpDisplayData.getUpgradeBoostValue(powerUpType).ToString("N0") );
		//Replace the string <range> by the distance in meters gained by the upgrade
		descriptionString = descriptionString.Replace( "<range>", PowerUpDisplayData.getUpgradeBoostValue(powerUpType).ToString("N0") );
		description.text = descriptionString;

		upgradeLevel.value = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType );
		
		//Disable buy button if we have reached the maximum upgrade level
		if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL )
		{
			buyButton.interactable = false;
			buyButtonLabel.alignment = TextAnchor.MiddleCenter;
			Image[] images = buyButton.GetComponentsInChildren<Image>();
			images[1].gameObject.SetActive( false );
			buyButtonLabel.text = LocalizationManager.Instance.getText("POWER_UP_BUTTON_MAXED_OUT");
			description.text = LocalizationManager.Instance.getText("POWER_UP_MAXIMUM_UPGRADE");
		}
		else
		{
			//Update the cost for the next purchase
			buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* price).ToString("N0");
		}
	}

	void initializeConsumableEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
		//Replace the string <quantity> by the quantity the player owns
		descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );
		description.text = descriptionString;

		buyButtonLabel.text = price.ToString("N0");
	}

	void initializeCoinDoublerEntry()
	{
		description.text = LocalizationManager.Instance.getText( "COIN_DOUBLER_DESCRIPTION" );
		if( PlayerStatsManager.Instance.getOwnsCoinDoubler() )
		{
			//Player already owns it
			buyButtonLabel.alignment = TextAnchor.MiddleCenter;
			buyButtonLabel.text = LocalizationManager.Instance.getText("COIN_DOUBLER_OWNED");
			buyButton.interactable = false;
		}
		else
		{
			buyButtonLabel.text = currencySymbol + price.ToString();
		}
	}

	void initializePurchaseLivesEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("STORE_ITEM_LIVES_DESCRIPTION");
		//Replace the string <quantity> by the quantity the player will receive if he makes the purchase
		descriptionString = descriptionString.Replace( "<quantity>", quantity.ToString("N0") );
		description.text = descriptionString;
		
		buyButtonLabel.text = currencySymbol + price.ToString();
	}

	void initializePurchaseCoinsEntry()
	{
		description.text = string.Format( LocalizationManager.Instance.getText("STORE_ITEM_COINS_DESCRIPTION"), quantity.ToString("N0") );
		buyButtonLabel.text = currencySymbol + price.ToString();
	}

	void initializePurchaseGemsEntry()
	{
		description.text = string.Format( LocalizationManager.Instance.getText("STORE_ITEM_GEMS_DESCRIPTION"), quantity.ToString("N0") );
		buyButtonLabel.text = currencySymbol + price.ToString();
	}

	void initializeRestoreCoinDoublerEntry()
	{
		description.text = LocalizationManager.Instance.getText("RESTORE_PURCHASE_DESCRIPTION");
		buyButtonLabel.text = LocalizationManager.Instance.getText("RESTORE_PURCHASE_BUTTON_LABEL");
	}

	public void buy()
	{
		switch (purchaseType)
		{
	        case PurchaseType.Upgrade:
				buyUpgrade();
                break;
	                
	        case PurchaseType.Consumable:
				buyConsumable();
                break;
	                
	        case PurchaseType.Purchase_Coin_Doubler:
				buyCoinDoubler();
                break;
	                
	        case PurchaseType.Purchase_Lives:
				buyLives();
                break;
	
	        case PurchaseType.Purchase_Coins:
				buyCoins();
                break;

	        case PurchaseType.Purchase_Gems:
				buyGems();
                break;

			 case PurchaseType.Restore_Coin_Doubler:
				restorePurchases();
                break;

			 case PurchaseType.None:
				Debug.LogError( title.text + " The PurchaseType parameter has not been set. A value of NONE is not permitted.");
                break;
		}
	}

	void buyUpgrade()
	{
		int newUpgradeValue = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1;
		int currentUpgradeCost = newUpgradeValue * (int) price;
		//Make sure we have enough currency
		if( currentUpgradeCost <= PlayerStatsManager.Instance.getCurrentCoins() )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			//Deduct the appropriate number of currency for the purchase
			PlayerStatsManager.Instance.modifyCurrentCoins(-currentUpgradeCost, false, true );
			print ( "buying new value is: " + newUpgradeValue );
			PlayerStatsManager.Instance.setPowerUpUpgradeLevel( powerUpType, newUpgradeValue  );
			upgradeLevel.value = newUpgradeValue;
			//Disable buy button if we have reached the maximum upgrade level
			if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL )
			{
				buyButton.interactable = false;
				Image[] images = buyButton.GetComponentsInChildren<Image>();
				images[1].gameObject.SetActive( false );
				buyButtonLabel.alignment = TextAnchor.MiddleCenter;
				buyButtonLabel.text = LocalizationManager.Instance.getText("POWER_UP_BUTTON_MAXED_OUT");
				description.text = LocalizationManager.Instance.getText("POWER_UP_MAXIMUM_UPGRADE");
			}
			else
			{
				//Update the cost for the next purchase
				buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* price).ToString("N0");
			}
			
			//Save the data since we spent some currency as well as upgraded a powerup.
			PlayerStatsManager.Instance.savePlayerStats();
			
		}
		else
		{
			//Player does not have enough coins.
		}
	}

	void buyConsumable()
	{
		//Make sure we have enough currency
		if( PlayerStatsManager.Instance.getCurrentCoins() >= price )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			//Deduct the appropriate number of currency for the purchase
			PlayerStatsManager.Instance.modifyCurrentCoins((int)(-price), false, true );
			PlayerStatsManager.Instance.incrementPowerUpInventory( powerUpType );
		
			string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
			//Replace the string <quantity> by the quantity the player owns
			descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );
			description.text = descriptionString;

			//Save the data since we spent some currency as well as upgraded a powerup.
			PlayerStatsManager.Instance.savePlayerStats();
			
		}
		else
		{
			//Player does not have enough coins.
		}
	}

	//This is a real money purchase
	void buyCoinDoubler()
	{
		Debug.Log("buyCoinDoubler");
		UISoundManager.uiSoundManager.playButtonClick();
		buyButtonLabel.alignment = TextAnchor.MiddleCenter;
		buyButtonLabel.text = LocalizationManager.Instance.getText("COIN_DOUBLER_OWNED");
		buyButton.interactable = false;

		//Grant the Coin Doubler
		PlayerStatsManager.Instance.setOwnsCoinDoubler( true );

		//Save the data
		PlayerStatsManager.Instance.savePlayerStats();
	}

	void buyLives()
	{
		Debug.Log("buyLives");
		UISoundManager.uiSoundManager.playButtonClick();

		//Grant the purchased lives
		PlayerStatsManager.Instance.increaseLives( quantity );

		//Save the data
		PlayerStatsManager.Instance.savePlayerStats();
	}

	void buyCoins()
	{
		Debug.Log("buyCoins");
		UISoundManager.uiSoundManager.playButtonClick();
		GameManager.Instance.playerInventory.addCoins( quantity );
	}

	void buyGems()
	{
		Debug.Log("buyGems");
		UISoundManager.uiSoundManager.playButtonClick();
		GameManager.Instance.playerInventory.addGems( quantity );
	}

	void restorePurchases()
	{
		Debug.LogWarning("restorePurchases-Not implemented");
	}


}
