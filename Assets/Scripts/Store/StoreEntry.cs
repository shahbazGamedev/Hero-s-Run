using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum PowerUpPurchaseType {
	
	None = 0,
	Upgrade = 1,
	Consumable = 2
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
	public Text playerCurrency;
	const int COST_FOR_ONE_CONSUMABLE = 1000;

	// Use this for initialization
	void Awake () {
	
		//For debugging, so I can see the text displayed without going through the load menu as well as have valid save data
		LocalizationManager.Instance.initialize(); 
		PlayerStatsManager.Instance.loadPlayerStats();

		//We have 4 Boosts
		//Star Magnet
		title.text = LocalizationManager.Instance.getText(titleID);

		if( powerUpPurchaseType == PowerUpPurchaseType.Upgrade )
		{
			initializeUpgradeEntry();
		}
		else if( powerUpPurchaseType == PowerUpPurchaseType.Consumable )
		{
			initializeConsumableEntry();
		}
		else
		{
			Debug.LogError("The powerUpPurchaseType parameter has not been set. A value of NONE is not permitted.");
		}

	
	}

	void initializeUpgradeEntry()
	{
		description.text = LocalizationManager.Instance.getText(descriptionID);
		
		buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* 1000).ToString("N0");
		
		upgradeLevel.value = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType );
		
		//Disable the buy button if we are already at the maximum upgrade level
		if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL ) buyButton.interactable = false;
	}

	void initializeConsumableEntry()
	{
		string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
		//Replace the string <quantity> by the quantity the player owns
		descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );

		buyButtonLabel.text = COST_FOR_ONE_CONSUMABLE.ToString("N0");
		description.text = descriptionString;
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
			if( playerCurrency != null ) playerCurrency.text = ( PlayerStatsManager.Instance.getLifetimeCoins() ).ToString("N0");;
			print ( "buying new value is: " + newUpgradeValue );
			PlayerStatsManager.Instance.setPowerUpUpgradeLevel( powerUpType, newUpgradeValue  );
			upgradeLevel.value = newUpgradeValue;
			//Disable buy button if we have reached the maximum upgrade level
			if( upgradeLevel.value == MAXIMUM_UPGRADE_LEVEL ) buyButton.interactable = false;
			//Update the cost for the next purchase
			buyButtonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* 1000).ToString("N0");
			
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
			if( playerCurrency != null ) playerCurrency.text = ( PlayerStatsManager.Instance.getLifetimeCoins() ).ToString("N0");;
			PlayerStatsManager.Instance.incrementPowerUpInventory( powerUpType );
		
			string descriptionString = LocalizationManager.Instance.getText("POWER_UP_YOU_HAVE");
			//Replace the string <quantity> by the quantity the player owns
			descriptionString = descriptionString.Replace( "<quantity>", PlayerStatsManager.Instance.getPowerUpQuantity(powerUpType).ToString("N0") );
			description.text = descriptionString;

			//Save the data since we spent some currency as well as upgraded a powerup.
			PlayerStatsManager.Instance.savePlayerStats();
			
		}
	}

}
