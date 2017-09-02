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
	/// Creates a new player deck containing the cards the players has after a new install. All of the cards are level 1. The player has 1 card of each.
	/// </summary>
	public void createNewPlayerDeck()
	{
		//Each Hero has a unique card. All heroes are unlocked immediately.
		//The card for the default hero (index of 0) is added to the deck first
		//because we want the Hero card to always appear in the top-left corner of the battle deck menu.
		//All other Hero cards are added as part of the Card Collection (but not as part of the Battle Deck).
		List<CardName> heroCardsList = HeroManager.Instance.getHeroCards();
		for( int i = 0; i < heroCardsList.Count; i++ )
		{
			if( i == 0 )
			{
				addCard( heroCardsList[i], 1, 1, true, true );
			}
			else
			{
				addCard( heroCardsList[i], 1, 1, false, true );
			}
		}

		//Add the 7 default cards.
		List<CardManager.CardData> defaultCardsList = CardManager.Instance.getAllDefaultCards();
		
		//Verify configuration
		if( defaultCardsList.Count != 7 )
		{
			Debug.LogError("PlayerDeck-createNewPlayerDeck: There should be exactly 7 cards in the defaultCardsList. Configure properly in the CardManager. The current number of default cards is: " + defaultCardsList.Count );
		}
		for( int i = 0; i < defaultCardsList.Count; i++ )
		{
			addCard( defaultCardsList[i].name, 1, 1, true );
		}

		//If this is a debug build, add additional cards to the card collection to facilitate testing
		addAdditionalCardsForTesting();

		//We should have a total of 8 cards in the Battle Deck (1 + 7).
		serializePlayerDeck( true );
	}
	
	void addAdditionalCardsForTesting()
	{
		if( Debug.isDebugBuild )
		{
			addCard( CardName.Firewall, 2, 1, false );
			addCard( CardName.Health_Boost, 2, 1, false );
			addCard( CardName.Armor, 2, 1, false );
		}
	}

	/// <summary>
	/// Returns the complete list of cards owned by the player.
	/// </summary>
	/// <returns>The player's complete card deck.</returns>
	public List<CardManager.CardData> getPlayerCardDeck()
	{
		List<CardManager.CardData> completeCardDeck = new List<CardManager.CardData>();
		for( int i = 0; i < playerCardDataList.Count; i++ )
		{
			completeCardDeck.Add( CardManager.Instance.getCardByName( playerCardDataList[i].name ) );
		}
		return completeCardDeck;
	}

	/// <summary>
	/// Returns a list of cards in the player's battle deck.
	/// </summary>
	/// <returns>The battle deck.</returns>
	public List<PlayerCardData> getBattleDeck()
	{
		//Reset inBattleDeck to false for the current hero card. There should only ever be one card.
		playerCardDataList.Find( c => c.inBattleDeck == true && c.isHeroCard == true ).inBattleDeck = false;

		//Now, set inBattleDeck to true for the card of the currently selected hero
		int heroIndex = GameManager.Instance.playerProfile.selectedHeroIndex;
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( heroIndex );
		getCardByName( hero.reservedCard ).inBattleDeck = true;

		return playerCardDataList.FindAll( card => card.inBattleDeck == true );
	}
 
	/// <summary>
	/// Returns a list of all of the cards owned by the player that are not in his battle deck and that are not Hero cards sorted according to parameter specified.
	/// </summary>
	/// <returns>The card deck.</returns>
	public List<CardManager.CardData> getCardDeck( CardSortMode cardSortMode )
	{
		List<PlayerCardData> playerCardDeck = playerCardDataList.FindAll( card => card.inBattleDeck == false && card.isHeroCard == false );
		List<CardManager.CardData> cardDeck = new List<CardManager.CardData>();
		for( int i = 0; i < playerCardDeck.Count; i++ )
		{
			cardDeck.Add( CardManager.Instance.getCardByName( playerCardDeck[i].name ) );
		}

		if( cardSortMode == CardSortMode.BY_POWER_COST )
		{
			cardDeck.Sort((x, y) => x.manaCost.CompareTo(y.manaCost));
		}
		else if( cardSortMode == CardSortMode.BY_RARITY )
		{
			cardDeck.Sort((x, y) => x.rarity.CompareTo(y.rarity));
		}
		else
		{
			Debug.LogError("PlayerDeck-The card sort mode specified " + cardSortMode + " is not handled by getCardDeck."); 
		}
		return cardDeck;
	}

	public int getTotalNumberOfCards()
	{
		return playerCardDataList.Count;
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

	public PlayerCardData addCard(  CardName name, int level, int quantity, bool inBattleDeck, bool isHeroCard = false, bool isNew = false )
	{
		//Make sure the specified card exists
		if( CardManager.Instance.doesCardExist( name ) )
		{
			//Don't add duplicate cards
			if( playerCardDataList.Exists(playerCardData => playerCardData.name == name ) ) return null;
	
			PlayerCardData pcd = new PlayerCardData();
			pcd.name = name;
			pcd.level = level;
			pcd.quantity = quantity;
			pcd.inBattleDeck = inBattleDeck;
			pcd.isHeroCard = isHeroCard;
			pcd.isNew = isNew;
			playerCardDataList.Add(pcd);
			return pcd;
		}
		else
		{
			Debug.LogError("PlayerDeck-addCard: The card you are trying to add to the player deck does not exist: " + name );
			return null;
		}
	}

	public void changeCardQuantity(  PlayerCardData pcd, int quantityToAdd )
	{
		//Make sure the specified card exists in the deck
		if( doesCardExist( pcd.name ) )
		{
			pcd.quantity += quantityToAdd;
		}
		else
		{
			Debug.LogError("PlayerDeck-changeCardQuantity: The card you are trying to change isn't in the player's deck: " + pcd.name );
		}
	}

	/// <summary>
	/// Gets the favorite card.
	/// </summary>
	/// <returns>The favorite card.</returns>
	public CardName getFavoriteCard()
	{
		return playerCardDataList.OrderByDescending( entry => entry.timesUsed ).First().name;
	}

	public void changeInBattleDeckStatus(  CardName name, bool inBattleDeck )
	{
		//Make sure the specified card exists
		if( doesCardExist( name ) )
		{
			PlayerCardData pcd = playerCardDataList.Find(playerCardData => playerCardData.name == name);
			pcd.inBattleDeck = inBattleDeck;
		}
		else
		{
			Debug.LogError("PlayerDeck-changeInBattleDeckStatus: The card you are trying to modify is not in the player's deck: " + name );
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
			Debug.LogWarning("PlayerDeck-getCardByName: The card you requested does not exist: " + name );
			return null;
		}
	}

	public void upgradeCardByOneLevel( CardName name )
	{
		if( doesCardExist( name ) )
		{
			PlayerCardData playerCard = playerCardDataList.Find(playerCardData => playerCardData.name == name);
			CardManager.CardData card = CardManager.Instance.getCardByName( name );
			//Verify if not at maximum level before continuing
			int maxCardLevel = CardManager.Instance.getMaxCardLevelForThisRarity( card.rarity );
			if( playerCard.level + 1 <= maxCardLevel )
			{
				//We are okay to upgrade
				playerCard.level++;
				//Deduct the number of cards needed to upgrade
				playerCard.quantity -= CardManager.Instance.getNumberOfCardsRequiredForUpgrade( playerCard.level, card.rarity);
				//Upgrading gives XP
				GameManager.Instance.playerProfile.addToTotalXPEarned( CardManager.Instance.getXPGainedAfterUpgrading( playerCard.level, card.rarity ), true );
				//Save
				serializePlayerDeck( true );
			}
			else
			{
				Debug.LogWarning("PlayerDeck-This card " + name + " is already maxed out." );
			}
		}
		else
		{
			Debug.LogWarning("PlayerDeck-upgradeCardByOneLevel: The card you requested does not exist: " + name );
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
		public int timesUsed;
		public bool isHeroCard;
		public bool isNew;	
	}

}
