using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LootBoxMenu : MonoBehaviour, IPointerDownHandler {

	[SerializeField] GameObject lootPanel;
	[SerializeField] Image lootSprite;
	[SerializeField] TextMeshProUGUI lootContentText;
	[SerializeField] Sprite coinSprite;
	[SerializeField] Sprite gemSprite;
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
		lootPanel.SetActive (true );
		lootBox.print();
		GameManager.Instance.playerProfile.setLastFreeLootBoxOpenedTime( DateTime.UtcNow );
		//Schedule a local notification to remind the player of when his next free loot box will be available
		NotificationServicesHandler.Instance.scheduleFreeLootBoxNotification(240);
	}

	void giveLoot( LootBox.Loot loot )
	{
		switch( loot.type )
		{
			case LootType.COINS:
				GameManager.Instance.playerInventory.addCoins( loot.quantity );
				displayLootReceived( "+" + loot.quantity.ToString(), coinSprite );
			break;

			case LootType.GEMS:
				GameManager.Instance.playerInventory.addGems( loot.quantity );
				displayLootReceived( "+" + loot.quantity.ToString(), gemSprite );
			break;

			case LootType.CARDS:
				GameManager.Instance.playerDeck.addCardFromLootBox( loot.cardName, loot.quantity );
				CardManager.CardData cd = CardManager.Instance.getCardByName( loot.cardName );
				displayLootReceived( "+" + loot.quantity.ToString(), cd.icon );
			break;

			case LootType.PLAYER_ICON:
				GameManager.Instance.playerIcons.unlockPlayerIcon( loot.uniqueItemID );
				string iconName = LocalizationManager.Instance.getText( "PLAYER_ICON_" + loot.uniqueItemID.ToString() );
				displayLootReceived( iconName, ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( loot.uniqueItemID ).icon );
			break;
			
			default:
				Debug.LogError("Give loot content encountered an unknown loot type: " + loot.type );
			break;
		}
	}

	void displayLootReceived( string text, Sprite sprite )
	{
		lootSprite.sprite = sprite;
		lootContentText.text = text;
		lootPanel.SetActive (true );
	}

	public void OnPointerDown(PointerEventData data)
	{
		if( lootCounter < lootList.Count )
		{
			giveLoot( lootList[lootCounter] );
			lootCounter++;
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
