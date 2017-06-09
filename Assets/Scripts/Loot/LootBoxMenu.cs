using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LootBoxMenu : MonoBehaviour, IPointerDownHandler {

	[SerializeField] GameObject lootPanel;
	[SerializeField] CardUIDetails cardUIDetails;
	[SerializeField] Image lootSprite;
	[SerializeField] TextMeshProUGUI lootNameText;
	[SerializeField] TextMeshProUGUI lootAmountText;
	[SerializeField] TextMeshProUGUI lootCounterText;
	[SerializeField] Sprite coinSprite;
	[SerializeField] Sprite gemSprite;
	[SerializeField] GameObject rarityPanel;
	[SerializeField] TextMeshProUGUI rarityText;
	[SerializeField] Image rarityIcon;

	[Header("Level")]
	[Tooltip("The level text is displayed on top of the card image. For example: 'Level 5' or 'Max Level'. The text color varies with the card rarity.")]
	[SerializeField] TextMeshProUGUI levelText;
	List<LootBox.Loot> lootList;
	int lootCounter = 0;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
	}

	void OnEnable()
	{
		LootBoxClientManager.lootBoxGrantedEvent += LootBoxGrantedEvent;
	}

	void OnDisable()
	{
		LootBoxClientManager.lootBoxGrantedEvent -= LootBoxGrantedEvent;
	}

	public void LootBoxGrantedEvent( LootBox lootBox )
	{
		lootList = lootBox.getLootList();
		lootBox.print();
		GameManager.Instance.playerProfile.setLastFreeLootBoxOpenedTime( DateTime.UtcNow );
		//Schedule a local notification to remind the player of when his next free loot box will be available
		NotificationServicesHandler.Instance.scheduleFreeLootBoxNotification(240);
		//Display the number of loot items in the loot box
		lootCounterText.text = lootList.Count.ToString();
		giveLoot( lootList[lootCounter] );
		lootCounter++;
		lootPanel.SetActive (true );
	}

	void giveLoot( LootBox.Loot loot )
	{
		switch( loot.type )
		{
			case LootType.COINS:
				rarityPanel.SetActive( false );
				lootNameText.text = LocalizationManager.Instance.getText( "STORE_COINS_TITLE" );
				GameManager.Instance.playerInventory.addCoins( loot.quantity );
				displayLootReceived( "+" + loot.quantity.ToString(), coinSprite );
			break;

			case LootType.GEMS:
				rarityPanel.SetActive( false );
				lootNameText.text = LocalizationManager.Instance.getText( "STORE_GEMS_TITLE" );
				GameManager.Instance.playerInventory.addGems( loot.quantity );
				displayLootReceived( "+" + loot.quantity.ToString(), gemSprite );
			break;

			case LootType.CARDS:
				rarityPanel.SetActive( true );
				GameManager.Instance.playerDeck.addCardFromLootBox( loot.cardName, loot.quantity );
				CardManager.CardData cd = CardManager.Instance.getCardByName( loot.cardName );
				PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( loot.cardName );
				cardUIDetails.configureCard( pcd, cd );
				//Card name
				string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
				lootNameText.text = localizedCardName;
				displayLootReceived( "+" + loot.quantity.ToString() );
				//Level section
				//Level background
				Color rarityColor;
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				if( levelText != null ) levelText.color = rarityColor;
		
				//Level text
				if( pcd.level + 1 < CardManager.Instance.getMaxCardLevelForThisRarity( cd.rarity ) )
				{
					if( levelText != null ) levelText.text = String.Format( LocalizationManager.Instance.getText( "CARD_LEVEL"), pcd.level.ToString() );
				}
				else
				{
					if( levelText != null ) levelText.text = LocalizationManager.Instance.getText( "CARD_MAX_LEVEL");
				}
				//Rarity
				ColorUtility.TryParseHtmlString (CardManager.Instance.getCardColorHexValue(cd.rarity), out rarityColor);
				rarityIcon.color = rarityColor;
				rarityText.text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString() );
			break;

			case LootType.PLAYER_ICON:
				rarityPanel.SetActive( false );
				lootNameText.text = LocalizationManager.Instance.getText( "PLAYER_ICON_MENU_TITLE" );
				GameManager.Instance.playerIcons.unlockPlayerIcon( loot.uniqueItemID );
				string iconName = LocalizationManager.Instance.getText( "PLAYER_ICON_" + loot.uniqueItemID.ToString() );
				displayLootReceived( iconName, ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( loot.uniqueItemID ).icon );
			break;
			
			default:
				Debug.LogError("Give loot content encountered an unknown loot type: " + loot.type );
			break;
		}
	}

	void displayLootReceived( string text, Sprite sprite = null )
	{
		if( sprite != null ) lootSprite.sprite = sprite;
		lootAmountText.text = text;
	}

	public void OnPointerDown(PointerEventData data)
	{
		if( lootCounter < lootList.Count )
		{
			giveLoot( lootList[lootCounter] );
			lootCounter++;
			lootCounterText.text = (lootList.Count - lootCounter).ToString();

		}
		else
		{
			//Save
			GameManager.Instance.playerInventory.serializePlayerInventory();
			GameManager.Instance.playerDeck.serializePlayerDeck(true);
			lootPanel.SetActive (false );
		}
	}

}
