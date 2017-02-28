﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
/// <summary>
/// Player deck.
/// </summary>
public class PlayerDeck {

	[SerializeField] List<PlayerCardData> playerCardDataList = new List<PlayerCardData>();


	public void initialiseForTesting()
	{
		//Cards in battle deck
		//1
		addCard( "Barbarians", 6, 174, true );

		//2
		addCard( "Dark Prince", 6, 174, true );

		//3
		addCard( "Furnace", 6, 174, true );

		//4
		addCard( "Giant Skeleton", 6, 174, true );

		//5
		addCard( "Goblin Hut", 6, 174, true );

		//6
		addCard( "Goblins", 6, 174, true );

		//7
		addCard( "Ice Spirit", 6, 174, true );

		//8
		addCard( "Inferno Tower", 6, 174, true );

		//Part of card collection, but not in battle deck
		//9
		addCard( "Lumberjack", 6, 174, false );

		//10
		addCard( "Mini-PEKKA", 6, 174, false );

		//11
		addCard( "Minions", 6, 174, false );

		//12
		addCard( "Mortar", 6, 174, false );

		serializePlayerDeck( true );

	}

	public List<PlayerCardData> getBattleDeck()
	{
		List<PlayerCardData> battleDeck = playerCardDataList.FindAll( card => card.inBattleDeck == true );
		Debug.Log("Cards in battle deck:\n" );
		for( int i = 0; i < battleDeck.Count; i++ )
		{
			Debug.Log("Card " + i + " " +  battleDeck[i].name );
		}
		return battleDeck;
	}
 
	public void addCard(  string name, int level, int quantity, bool inBattleDeck )
	{
		//Make sure the specified card exists
		if( CardManager.Instance.doesCardExist( name ) )
		{
			//Don't add duplicate cards
			if( playerCardDataList.Exists(playerCardData => playerCardData.name == name ) ) return;
	
			PlayerCardData pcd = new PlayerCardData();
			pcd.name = name;
			pcd.level = level;
			pcd.quantity = quantity;
			pcd.inBattleDeck = inBattleDeck;
			playerCardDataList.Add(pcd);
		}
		else
		{
			Debug.LogError("PlayerDeck-addCard: The card you are trying to add to the player deck does not exist: " + name );
		}
	}

	public bool doesCardExist( string name )
	{
		return playerCardDataList.Exists(cardData => cardData.name == name );
	}

	public PlayerCardData getCardByName( string name )
	{
		if( doesCardExist( name ) )
		{
			return playerCardDataList.Find(playerCardData => playerCardData.name == name);
		}
		else
		{
			Debug.LogError("PlayerDeck-getCardByName: The card you requested does not exist: " + name );
			return null;
		}
	}

	public void serializePlayerDeck( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerDeck( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	/// <summary>
	/// Card data. The card data only handles data that never changes.
	/// </summary>
	[System.Serializable]
	public class PlayerCardData
	{
		public string name; 
		[Range(1,13)]
		public int level;
		public int  quantity;
		public bool inBattleDeck;		
	}

}
