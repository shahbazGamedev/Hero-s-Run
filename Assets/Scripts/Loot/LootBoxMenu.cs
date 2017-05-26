using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxMenu : MonoBehaviour {

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
		lootBox.print();
		giveLootBoxContent( lootBox );
	}

	void giveLootBoxContent( LootBox lootBox )
	{
		List<LootBox.Loot> lootList = lootBox.getLootList();
		for( int i = 0; i < lootList.Count; i++ )
		{
			switch( lootList[i].type )
			{
				case LootType.COINS:
					GameManager.Instance.playerInventory.addCoins( lootList[i].quantity );
				break;

				case LootType.GEMS:
					GameManager.Instance.playerInventory.addGems( lootList[i].quantity );
				break;

				case LootType.CARDS:
					GameManager.Instance.playerDeck.addCardFromLootBox( lootList[i].cardName, lootList[i].quantity );
				break;

				case LootType.PLAYER_ICON:
					GameManager.Instance.playerIcons.unlockPlayerIcon( lootList[i].uniqueItemID );
				break;
				
				default:
					Debug.LogError("Give loot content encountered an unknown loot type: " + lootList[i].type );
				break;
			}
		}
		//Save
		GameManager.Instance.playerInventory.serializePlayerInventory();
		GameManager.Instance.playerDeck.serializePlayerDeck(true);
	}

}
