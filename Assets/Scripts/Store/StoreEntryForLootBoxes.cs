using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class StoreEntryForLootBoxes : MonoBehaviour {

	[Header("Store Entry For Loot Box")]
	[SerializeField] LootBoxType lootBoxType;
	[SerializeField] TextMeshProUGUI currentRaceTrackText;
	[SerializeField] TextMeshProUGUI lootBoxNameText;
	[SerializeField] TextMeshProUGUI buyWithGemsButtonLabel;
	[SerializeField] int priceInGems;

	[SerializeField] StoreNotEnoughGemsPopup storeNotEnoughGemsPopup;

	// Use this for initialization
	void Start ()
	{
		lootBoxNameText.text = LocalizationManager.Instance.getText("STORE_ITEM_" + lootBoxType.ToString() );
		string sectorName = LocalizationManager.Instance.getText( "SECTOR_" + GameManager.Instance.playerProfile.getCurrentSector().ToString() );
		currentRaceTrackText.text = sectorName;
		buyWithGemsButtonLabel.text = priceInGems.ToString("N0");
		setTextColorBasedOnGemBalance( GameManager.Instance.playerInventory.getGemBalance() );
	}

	void OnEnable()
	{
		PlayerInventory.playerInventoryChangedNew += PlayerInventoryChangedNew;
	}

	void OnDisable()
	{
		PlayerInventory.playerInventoryChangedNew -= PlayerInventoryChangedNew;
	}

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int previousValue, int newValue )
	{
		switch (eventType)
		{
			case PlayerInventoryEvent.Gem_Balance_Changed:
				setTextColorBasedOnGemBalance( newValue );
			break;
		}
	}

	void setTextColorBasedOnGemBalance( int gemBalance )
	{
		if( priceInGems <= gemBalance )
		{
			//We can afford it.
			buyWithGemsButtonLabel.color = Color.white;
		}
		else
		{
			//We can't afford it.
			buyWithGemsButtonLabel.color = Color.red;
		}
	}

	public void buy()
	{
		Debug.Log("Buy Loot Box: " + lootBoxType.ToString() );
		UISoundManager.uiSoundManager.playButtonClick();

		if( priceInGems <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			//We can afford it.
			GameManager.Instance.playerInventory.deductGems( priceInGems );
			LootBoxClientManager.Instance.requestLootBox(lootBoxType);
		}
		else
		{
			//We can't afford it. Offer to go to shop.
			storeNotEnoughGemsPopup.Show();
		}
	}
}
