using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootBoxType
 {
	FREE = 0,
	SHOP_GIANT = 1,
	SHOP_SUPER_SIZED = 2,
	SHOP_MEGA = 3,
	CROWN = 4,
	LEVEL_UP = 5,
	NEW_RACE_TRACK_UNLOCKED = 6
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
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting from the server a loot box of type " + lootBoxType );
		string lootBoxJson = string.Empty;
		switch (lootBoxType)
		{
			case LootBoxType.FREE:
				lootBoxJson = getFreeLootBox( raceTrackLevel );
			break;
			case LootBoxType.SHOP_GIANT:
				lootBoxJson = getGiantLootBox( raceTrackLevel );
			break;
			case LootBoxType.SHOP_SUPER_SIZED:
				lootBoxJson = getSupersizedLootBox( raceTrackLevel );
			break;
			case LootBoxType.SHOP_MEGA:
				lootBoxJson = getMegaLootBox( raceTrackLevel );
			break;
			case LootBoxType.CROWN:
				lootBoxJson = getCrownLootBox( raceTrackLevel );
			break;
		}

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

		return lootBox.getJson();
	}

	/*
	Giant chests guarantee a large number of Rare Cards and Common Cards and have a high chance of containing an Epic Card.
	Giant chests take 12 hours or 72 gems to unlock.
	Giant chests can be purchased in the Shop and are the cheapest chest (not counting special offers).
	The cost depends on the Arena the player is in. The higher the Arena, the more expensive it will be, though it will guarantee more cards in higher Arenas.
	*/
	string getGiantLootBox( int raceTrackLevel )
	{
		LootBox lootBox = new LootBox();

		LootBox.Loot loot = new LootBox.Loot();
		loot.type = LootType.COINS;
		loot.quantity = Random.Range(40,51);
		lootBox.addLoot( loot );

		loot = new LootBox.Loot();
		loot.type = LootType.CARDS;
		loot.cardName = CardManager.Instance.getRandomCard( raceTrackLevel, CardRarity.COMMON );
		loot.quantity = 50;
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

		loot = new LootBox.Loot();
		loot.type = LootType.PLAYER_ICON;
		loot.uniqueItemID = ProgressionManager.Instance.getRandomPlayerIconUniqueId();
		//To do
		//If the player already has that player icon, convert to 15 coins.
		lootBox.addLoot( loot );

		loot = new LootBox.Loot();
		loot.type = LootType.VOICE_LINE;
		int randomSex = Random.Range( 0, 2 );
		loot.uniqueItemID = VoiceOverManager.Instance.getRandomTaunt ( (Sex) randomSex );
		loot.sex = (Sex) randomSex;
		lootBox.addLoot( loot );

		return lootBox.getJson();
	}

	string getSupersizedLootBox( int raceTrackLevel )
	{
		LootBox lootBox = new LootBox();

		LootBox.Loot loot = new LootBox.Loot();
		loot.type = LootType.COINS;
		loot.quantity = Random.Range(40,51);
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

		loot = new LootBox.Loot();
		loot.type = LootType.PLAYER_ICON;
		loot.uniqueItemID = ProgressionManager.Instance.getRandomPlayerIconUniqueId();
		//To do
		//If the player already has that player icon, convert to 15 coins.
		lootBox.addLoot( loot );

		return lootBox.getJson();
	}

	string getMegaLootBox( int raceTrackLevel )
	{
		LootBox lootBox = new LootBox();

		LootBox.Loot loot = new LootBox.Loot();
		loot.type = LootType.COINS;
		loot.quantity = Random.Range(40,51);
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

		loot = new LootBox.Loot();
		loot.type = LootType.PLAYER_ICON;
		loot.uniqueItemID = ProgressionManager.Instance.getRandomPlayerIconUniqueId();
		//To do
		//If the player already has that player icon, convert to 15 coins.
		lootBox.addLoot( loot );

		return lootBox.getJson();
	}

	string getCrownLootBox( int raceTrackLevel )
	{
		LootBox lootBox = new LootBox();

		LootBox.Loot loot = new LootBox.Loot();
		loot.type = LootType.COINS;
		loot.quantity = Random.Range(140,160);
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

		loot = new LootBox.Loot();
		loot.type = LootType.PLAYER_ICON;
		loot.uniqueItemID = ProgressionManager.Instance.getRandomPlayerIconUniqueId();
		//To do
		//If the player already has that player icon, convert to 15 coins.
		lootBox.addLoot( loot );

		return lootBox.getJson();
	}

}
