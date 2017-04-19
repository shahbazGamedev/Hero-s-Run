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

	/// <summary>
	/// Creates the new player deck. This has the cards the players has after a new install. All of the cards are level 1. The player has 1 card of each.
	/// </summary>
	public void createNewPlayerDeck()
	{
		//Cards in battle deck
		//1
		addCard( CardName.IceWall, 18, 1000, true );

		//2
		addCard( CardName.Sprint, 1, 1, true );

		//3
		addCard( CardName.Raging_Bull, 1, 54, true );

		//4
		addCard( CardName.Explosion, 4, 12, true );

		//5
		addCard( CardName.Stasis, 1, 5, true );

		//6
		addCard( CardName.Lightning, 5, 1, true );

		//7
		addCard( CardName.Shrink, 3, 1, true );

		//8
		addCard( CardName.Sentry, 1, 69, true );

		//Part of card collection, but not in battle deck
		//9
		addCard( CardName.Card_One, 1, 1, false );

		//10
		addCard( CardName.Card_Two, 1, 1, false );

		//11
		addCard( CardName.Card_Three, 1, 1, false );

		//12
		addCard( CardName.Card_Four, 1, 1, false );

		serializePlayerDeck( true );

	}

	/// <summary>
	/// Returns a list of cards in the player's battle deck.
	/// </summary>
	/// <returns>The battle deck.</returns>
	public List<PlayerCardData> getBattleDeck()
	{
		return playerCardDataList.FindAll( card => card.inBattleDeck == true );
	}
 
	/// <summary>
	/// Returns a list of all of the cards owned by the player that are not in his battle deck.
	/// </summary>
	/// <returns>The card deck.</returns>
	public List<PlayerCardData> getCardDeck()
	{
		return playerCardDataList.FindAll( card => card.inBattleDeck == false );
	}

	public float getAverageManaCost()
	{
		List<PlayerCardData> battleDeck = getBattleDeck();
		float totalBattleDeckMana = 0;
		for( int i = 0; i < battleDeck.Count; i++ )
		{
			CardManager.CardData cd = CardManager.Instance.getCardByName( battleDeck[i].name );
			totalBattleDeckMana += cd.manaCost;
		}
		return totalBattleDeckMana/battleDeck.Count;
	}

	public void addCard(  CardName name, int level, int quantity, bool inBattleDeck )
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

	public bool doesCardExist( CardName name )
	{
		return playerCardDataList.Exists(cardData => cardData.name == name );
	}

	public PlayerCardData getCardByName( CardName name )
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
		public CardName name; 
		[Range(1,13)]
		public int level;
		public int  quantity;
		public bool inBattleDeck;		
	}

}
