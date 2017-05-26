using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootBoxType
 {
	FREE = 0,
	CROWNS = 1,
	SHOP = 2,
	RACE_TRACK_UNLOCK = 3
}

public class LootBoxServerManager : MonoBehaviour {

	public static LootBoxServerManager Instance;

	// Use this for initialization
	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

		public void requestLootBox( LootBoxType lootBoxType, int raceTrackLevel )
	{
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a server loot box of type " + lootBoxType );
		string lootBoxJson = getFreeLootBox( raceTrackLevel );
		LootBoxClientManager.Instance.lootBoxGranted( lootBoxJson );
	}

	string getFreeLootBox( int raceTrackLevel )
	{
		LootBox lootBox = new LootBox();

		LootBox.Loot loot = new LootBox.Loot();
		loot.type = LootType.COINS;
		loot.quantity = Random.Range(40,51);
		lootBox.addLoot( loot );

		loot = new LootBox.Loot();
		loot.type = LootType.GEMS;
		loot.quantity = Random.Range(2,4);
		lootBox.addLoot( loot );

		loot = new LootBox.Loot();
		loot.type = LootType.CARDS;
		loot.cardName = CardManager.Instance.getRandomCard( raceTrackLevel, CardRarity.COMMON );
		loot.quantity = 5;
		lootBox.addLoot( loot );

		loot = new LootBox.Loot();
		loot.type = LootType.CARDS;
		CardRarity rarity;
		if( raceTrackLevel <= 3 )
		{
			rarity = CardRarity.RARE;
		}
		else
		{
			rarity = CardRarity.EPIC;
		}
		loot.cardName = CardManager.Instance.getRandomCard( raceTrackLevel, rarity );
		loot.quantity = 1;
		lootBox.addLoot( loot );

		//For testing - there are NO player Icons in the free loot boxes
		loot = new LootBox.Loot();
		loot.type = LootType.PLAYER_ICON;
		loot.uniqueItemID = ProgressionManager.Instance.getRandomPlayerIconUniqueId();
		//To do
		//If the player already has that player icon, convert to 15 coins.
		lootBox.addLoot( loot );

		return lootBox.getJson();
	}

}
