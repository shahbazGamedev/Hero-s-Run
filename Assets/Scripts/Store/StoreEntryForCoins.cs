using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoreEntryForCoins : MonoBehaviour {

	[Header("Store Entry For Coins")]
	[SerializeField]  Text title;
	[SerializeField]  string titleID;
	[Tooltip("For example, the number of coins you get with your purchase.")]
	[SerializeField]  Text quantityText;
	[SerializeField]  int quantity;
	[SerializeField] Text buyWithGemsButtonLabel;
	[SerializeField] int priceInGems;

	[SerializeField] StoreNotEnoughGemsPopup storeNotEnoughGemsPopup;

	// Use this for initialization
	void Start ()
	{
		title.text = LocalizationManager.Instance.getText(titleID);
		buyWithGemsButtonLabel.text = priceInGems.ToString("N0");
		quantityText.text = quantity.ToString("N0");
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

	void PlayerInventoryChangedNew( PlayerInventoryEvent eventType, int newValue )
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
		Debug.Log("Buy " + quantity + " coins.");
		UISoundManager.uiSoundManager.playButtonClick();
		if( priceInGems <= GameManager.Instance.playerInventory.getGemBalance() )
		{
			//We can afford it.
			GameManager.Instance.playerInventory.deductGems( priceInGems );
			GameManager.Instance.playerInventory.addCoins( quantity );
		}
		else
		{
			//We can't afford it. Offer to go to shop.
			storeNotEnoughGemsPopup.Show();
		}
	}
}
