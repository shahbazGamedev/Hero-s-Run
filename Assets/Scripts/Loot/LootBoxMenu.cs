using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootBoxMenu : MonoBehaviour {

	[SerializeField] GameObject lootPanel;
	[SerializeField] Image lootSprite;
	[SerializeField] TextMeshProUGUI lootContentText;
	[SerializeField] Sprite coinSprite;
	[SerializeField] Sprite gemSprite;

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
		StartCoroutine( giveLootBoxContent( lootBox ) );
	}

	IEnumerator giveLootBoxContent( LootBox lootBox )
	{
		List<LootBox.Loot> lootList = lootBox.getLootList();
		for( int i = 0; i < lootList.Count; i++ )
		{
			switch( lootList[i].type )
			{
				case LootType.COINS:
					GameManager.Instance.playerInventory.addCoins( lootList[i].quantity );
					displayLootReceived( "+" + lootList[i].quantity.ToString(), coinSprite );
				break;

				case LootType.GEMS:
					GameManager.Instance.playerInventory.addGems( lootList[i].quantity );
					displayLootReceived( "+" + lootList[i].quantity.ToString(), gemSprite );
				break;

				case LootType.CARDS:
					GameManager.Instance.playerDeck.addCardFromLootBox( lootList[i].cardName, lootList[i].quantity );
					CardManager.CardData cd = CardManager.Instance.getCardByName( lootList[i].cardName );
					displayLootReceived( "+" + lootList[i].quantity.ToString(), cd.icon );
				break;

				case LootType.PLAYER_ICON:
					GameManager.Instance.playerIcons.unlockPlayerIcon( lootList[i].uniqueItemID );
					string iconName = LocalizationManager.Instance.getText( "PLAYER_ICON_" + lootList[i].uniqueItemID.ToString() );
					displayLootReceived( iconName, ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( lootList[i].uniqueItemID ).icon );
				break;
				
				default:
					Debug.LogError("Give loot content encountered an unknown loot type: " + lootList[i].type );
				break;
			}
			yield return new WaitForSecondsRealtime( 3f );
		}
		//Save
		GameManager.Instance.playerInventory.serializePlayerInventory();
		GameManager.Instance.playerDeck.serializePlayerDeck(true);
		lootPanel.SetActive (false );
	}

	void displayLootReceived( string text, Sprite sprite )
	{
		lootSprite.sprite = sprite;
		lootContentText.text = text;
		lootPanel.SetActive (true );
	}
}
