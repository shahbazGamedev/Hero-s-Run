using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum PowerUpPurchaseType {
	
	None = 0,
	Upgrade = 1,
	Consumable = 2,
	StarDoubler = 3,
	Purchase_Stars = 4,
	Purchase_Lives = 5,
	Restore_StarDoubler = 6
}

public class StoreEntry : MonoBehaviour {

	[Header("Store Entry")]
	public PowerUpType powerUpType = PowerUpType.None;
	public PowerUpPurchaseType powerUpPurchaseType = PowerUpPurchaseType.None;

	public Text title;
	public Text description;
	public Button buyButton;
	public Text buyButtonLabel;
	public Slider upgradeLevel;
	public string titleID = "POWER_UP_MAGNET";
	public string descriptionID = "POWER_UP_MAGNET_DESCRIPTION";
	//Valid upgrade values are integers between 0 to 6 included
	const int MAXIMUM_UPGRADE_LEVEL = 6;
	public Text totalStarsOwned;
	const int COST_FOR_ONE_CONSUMABLE = 1000;
	const string COST_STAR_DOUBLER = "$4.99";

	// Use this for initialization
	void Awake () {
	
		//So I can see the text displayed without going through the load menu as well as have valid save data while in the editor
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); 
		PlayerStatsManager.Instance.loadPlayerStats();
		#endif

		title.text = LocalizationManager.Instance.getText(titleID);

		if( powerUpPurchaseType == PowerUpPurchaseType.Upgrade )
		{
			initializeUpgradeEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.Consumable )
		{
			initializeConsumableEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.StarDoubler )
		{
			initializeStarDoublerEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.Purchase_Lives )
		{
			initializePurchaseLivesEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.Purchase_Stars )
		{
			initializePurchaseStarsEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.None )
		{
			Debug.LogError( title.text + " The powerUpPurchaseType parameter has not been set. A value of NONE is not permitted.");
		}

	
	}

	void initializeUpgradeEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText(descriptionID);
		//Replace the string <time> by the time in seconds gained by the upgrade
		descriptionString = descriptionString.Replace( "<time>", PowerUpManager.UPGRADE_DURATION_BOOST.ToString("N0") );
		//Replace the string <range> by the distance in meters gained by the upgrade
		descriptionString = descriptionString.Replace( "<range>", PowerUpManager.UPGRADE_DIAMETER_BOOST.ToString("N0") );
		description.text = descriptionString;

		upgradeLevel.value = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType );
		
		//Disable buy button if we have reached the maximum upgrade level
		if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL )
		{
			buyButton.gameObject.SetActive(false);
			description.text = LocalizationManager.Instance.getText("POWER_UP_MAXIMUM_UPGRADE");
		}
		else
		{
			//Update the cost for the next purchase
			buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* 1000).ToString("N0");
		}
	}

	void initializeConsumableEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
		//Replace the string <quantity> by the quantity the player owns
		descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );
		description.text = descriptionString;

		buyButtonLabel.text = COST_FOR_ONE_CONSUMABLE.ToString("N0");
	}

	void initializeStarDoublerEntry()
	{
		description.text = LocalizationManager.Instance.getText( "STAR_DOUBLER_DESCRIPTION" );
		if( PlayerStatsManager.Instance.getOwnsStarDoubler() )
		{
			//Player already owns it
			buyButtonLabel.text = LocalizationManager.Instance.getText("STAR_DOUBLER_OWNED");
			buyButton.interactable = false;
		}
		else
		{
			buyButtonLabel.text = COST_STAR_DOUBLER;
		}
	}

	void initializePurchaseLivesEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("STORE_ITEM_LIVES_DESCRIPTION");
		//Replace the string <quantity> by the quantity the player will receive if he makes the purchase
		descriptionString = descriptionString.Replace( "<quantity>", "25" );
		description.text = descriptionString;
		
		buyButtonLabel.text = COST_FOR_ONE_CONSUMABLE.ToString("N0");
	}

	void initializePurchaseStarsEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("STORE_ITEM_STARS_DESCRIPTION");
		//Replace the string <quantity> by the quantity the player will receive if he makes the purchase
		descriptionString = descriptionString.Replace( "<quantity>", "50" );
		description.text = descriptionString;
		
		buyButtonLabel.text = COST_FOR_ONE_CONSUMABLE.ToString("N0");
	}

	public void buy()
	{
		if( powerUpPurchaseType == PowerUpPurchaseType.Upgrade )
		{
			buyUpgrade();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.Consumable )
		{
			buyConsumable();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.StarDoubler )
		{
			buyStarDoubler();
		}
		else
		{
			Debug.LogError("The powerUpPurchaseType parameter has not been set. A value of NONE is not permitted.");
			return;
		}
	}

	void buyUpgrade()
	{
		int newUpgradeValue = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1;
		int currentUpgradeCost = newUpgradeValue * 1000;
		//Make sure we have enough currency
		if( currentUpgradeCost <= PlayerStatsManager.Instance.getLifetimeCoins() )
		{
			SoundManager.playButtonClick();
			//Deduct the appropriate number of currency for the purchase
			PlayerStatsManager.Instance.modifyCoinCount(-currentUpgradeCost);
			if( totalStarsOwned != null ) totalStarsOwned.text = ( PlayerStatsManager.Instance.getLifetimeCoins() ).ToString("N0");;
			print ( "buying new value is: " + newUpgradeValue );
			PlayerStatsManager.Instance.setPowerUpUpgradeLevel( powerUpType, newUpgradeValue  );
			upgradeLevel.value = newUpgradeValue;
			//Disable buy button if we have reached the maximum upgrade level
			if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL )
			{
				buyButton.gameObject.SetActive(false);
				description.text = LocalizationManager.Instance.getText("POWER_UP_MAXIMUM_UPGRADE");
			}
			else
			{
				//Update the cost for the next purchase
				buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* 1000).ToString("N0");
			}
			
			//Save the data since we spent some currency as well as upgraded a powerup.
			PlayerStatsManager.Instance.savePlayerStats();
			
		}
	}

	void buyConsumable()
	{
		//Make sure we have enough currency
		if( PlayerStatsManager.Instance.getLifetimeCoins() >= COST_FOR_ONE_CONSUMABLE )
		{
			SoundManager.playButtonClick();
			//Deduct the appropriate number of currency for the purchase
			PlayerStatsManager.Instance.modifyCoinCount(-COST_FOR_ONE_CONSUMABLE);
			if( totalStarsOwned != null ) totalStarsOwned.text = ( PlayerStatsManager.Instance.getLifetimeCoins() ).ToString("N0");;
			PlayerStatsManager.Instance.incrementPowerUpInventory( powerUpType );
		
			string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
			//Replace the string <quantity> by the quantity the player owns
			descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );
			description.text = descriptionString;

			//Save the data since we spent some currency as well as upgraded a powerup.
			PlayerStatsManager.Instance.savePlayerStats();
			
		}
	}

	//This is a real money purchase
	void buyStarDoubler()
	{
		SoundManager.playButtonClick();

		buyButtonLabel.text = LocalizationManager.Instance.getText("STAR_DOUBLER_OWNED");
		buyButton.interactable = false;

		//Grant the Star Doubler
		PlayerStatsManager.Instance.setOwnsStarDoubler( true );

		//Save the data
		PlayerStatsManager.Instance.savePlayerStats();
	}

}
