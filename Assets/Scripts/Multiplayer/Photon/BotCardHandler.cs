using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BotCardHandler : Photon.PunBehaviour {

	HeroManager.BotHeroCharacter botHero;
	HeroManager.BotSkillData botSkillData;
	List<PlayerDeck.PlayerCardData> battleDeckList;
	float manaAmount = ManaBar.START_MANA_POINT;
	CardHandler cardHandler;
	CardManager.CardData nextCard;
	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK);
	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();
	float timeOfLastAnalysis = 0;
	PlayerControl playerControl;
	PlayerSpell playerSpell;
	bool allowCardPlaying = false;

	// Use this for initialization
	void Start ()
	{
		//Get a reference to the CardHandler
		cardHandler = GameObject.FindGameObjectWithTag("Card Handler").GetComponent<CardHandler>();

		//Get and store the bot that was selected in MPNetworkLobbyManager and saved in LevelManager.
		botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );

		//Get and store the bot skill data for that hero.
		botSkillData = HeroManager.Instance.getBotSkillData( botHero.skillLevel );

		//Get and store the battle deck for this hero
		battleDeckList = getBattleDeck();

		//Get and store components
		playerControl = GetComponent<PlayerControl>();
		playerSpell = GetComponent<PlayerSpell>();

		initializeCards ();

		if( botHero.skillLevel != BotSkillLevel.VERY_LOW && LevelManager.Instance.allowBotToPlayCards ) Invoke("allowBotToPlayCards", botSkillData.raceStartGracePeriod );
	}

	void allowBotToPlayCards()
	{
		 allowCardPlaying = true;
	}

	List<PlayerDeck.PlayerCardData> getBattleDeck()
	{
		List<PlayerDeck.PlayerCardData> battleDeck = botHero.botCardDataList.FindAll( card => card.inBattleDeck == true );
		//Check that each card in the deck is unique. Maybe a data entry error caused 2 cards in the deck to be the same.
		bool isUnique = battleDeck.Select(card => card.name).Distinct().Count() == battleDeck.Count();
		if( !isUnique )
		{
			Debug.LogError("BotCardHandler-There are some duplicate cards in the battle deck for " + gameObject.name );
			for( int i = 0; i < battleDeck.Count; i++ )
			{
				Debug.Log("Battle deck contains " + i + " " +  battleDeck[i].name );
			}
			battleDeck = null;

		}
		return battleDeck;
	}
	
	// Use this for initialization
	void initializeCards ()
	{
		cardIndexList.AddRange(cardIndexArray);

		//Populate turn-ribbon with 3 or 4 unique, random cards
		for( int i = 0; i < CardManager.Instance.cardsInTurnRibbon; i++ )
		{
			addCardToTurnRibbon( i, battleDeckList[getUniqueRandom()].name );
		}
		//Set next card with a unique, random card
		setNextCard( battleDeckList[getUniqueRandom()].name );

		//We are testing with 3 and with 4 cards in the turn ribbon
		//So we are either queuing 4 cards: 8 - 3 - 1 or 3 cards: 8 - 4 - 1
		int cardsToQueue = TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK - CardManager.Instance.cardsInTurnRibbon - 1;
		for( int i = 0; i < cardsToQueue; i++ )
		{
			addCardToQueue( battleDeckList[getUniqueRandom()].name );
		}
	}

	void addCardToTurnRibbon( int index, CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		turnRibbonList.Add(cardData);
	}

	void setNextCard( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		nextCard = cardData;
	}

	void addCardToQueue( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		cardQueue.Enqueue( cardData );
	}

	// Update is called once per frame
	void Update ()
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			if( manaAmount < ManaBar.MAX_MANA_POINT ) manaAmount = manaAmount + Time.deltaTime/ManaBar.MANA_REFILL_RATE;
			if( allowCardPlaying && (Time.time - timeOfLastAnalysis > botSkillData.cardPlayFrequency) && !playerSpell.isAffectedByHack() )
			{
				analyseCards();
			}
		}
	}

	void playCard( int indexOfCardToPlay )
	{
		//Which card did the bot select?
		CardName cardName = turnRibbonList[indexOfCardToPlay].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );

		//Deduct the mana
		deductMana( playedCard.manaCost );

		//Activate the card
		activateCard( playedCard.name );

		//When you play the Steal card, it does not get replaced by the card in the Next slot but by the card
		//you are stealing.
		if( cardName != CardName.Steal )
		{
			//Wait a little before moving the Next Card into the free ribbon slot
			StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardToPlay, playedCard ) );
		}
	}

	IEnumerator moveNextCardIntoTurnRibbon( int indexOfCardToPlay, CardManager.CardData playedCard )
	{
		yield return new WaitForSecondsRealtime( TurnRibbonHandler.DELAY_BEFORE_NEXT_CARD_AVAILABLE );

		//In the turn-ribbon list, replace the card played by the card held in Next
		turnRibbonList.RemoveAt(indexOfCardToPlay);
		turnRibbonList.Insert( indexOfCardToPlay, nextCard );

		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		nextCard = oldestCardInQueue;

		//Enqueue the card that was just played
		cardQueue.Enqueue( playedCard );

	}

	public void activateCard( CardName cardName )
	{
		PlayerDeck.PlayerCardData botCardData = getCardByName( cardName );
		if( botCardData != null )
		{
			cardHandler.activateCard( this.photonView, cardName, botHero.name, botCardData.level );
		}
	}

	void deductMana( int manaCost )
	{
		if( manaAmount >= manaCost )
		{
			manaAmount = manaAmount - manaCost;
		}
		else
		{
			Debug.LogError("BotCardHandler-deductMana: insufficent mana.");
		}
	}

	bool doesCardExist( CardName name )
	{
		return battleDeckList.Exists(cardData => cardData.name == name );
	}

	PlayerDeck.PlayerCardData getCardByName( CardName name )
	{
		if( doesCardExist( name ) )
		{
			return battleDeckList.Find(playerCardData => playerCardData.name == name);
		}
		else
		{
			Debug.LogError("BotCardHandler-getCardByName: The card you requested does not exist: " + name );
			return null;
		}
	}

	int getUniqueRandom()
	{
		if(cardIndexList.Count == 0 )
		{
			Debug.LogError("BotCardHandler-getUniqueRandom: cardIndexList is empty.");
			return -1;
		}
		int rand = Random.Range(0, cardIndexList.Count);
		int value = cardIndexList[rand];
		cardIndexList.RemoveAt(rand);
		return value;
	}

	/// <summary>
	/// Attempt to play this card.
	/// If the bot has this card, is allowed to play cards, is not affected by Hack, and has enough mana, play the card.
	/// </summary>
	public void tryToPlayCard( CardName cardName )
	{
		if( allowCardPlaying && !playerSpell.isAffectedByHack() )
		{
			List<PlayerDeck.PlayerCardData> playableCardsList = getListOfPlayableCards();
			if( playableCardsList.Exists( card => card.name == cardName ) )
			{
				int indexOfCardToPlay = getCardIndexInTurnRibbon( cardName );
				playCard(indexOfCardToPlay);
			}
		}
	}

	#region Card Analysis
	void analyseCards()
	{
		//For the time being, we consider playing a card only in the RUNNING state.
		if( playerControl.getCharacterState() != PlayerCharacterState.Running ) return;

		timeOfLastAnalysis = Time.time;
		
		List<PlayerDeck.PlayerCardData> playableCardsList = getListOfPlayableCards();
		List<PlayerDeck.PlayerCardData> effectiveCardsList = new List<PlayerDeck.PlayerCardData>();

		//Do we have at least one playable card?
		if( playableCardsList.Count > 0 )
		{
			//Find effective cards, if any
			for( int i = 0; i < playableCardsList.Count; i++ )
			{
				if( cardHandler.isCardEffective( gameObject, playableCardsList[i].name, playableCardsList[i].level ) )
				{
					effectiveCardsList.Add( playableCardsList[i] );
				}
			}

			//Do we have at least one effective card?
			if( effectiveCardsList.Count > 0 )
			{
				//Among the effective cards, play one randomly
				int random = Random.Range(0,effectiveCardsList.Count);
				int indexOfCardToPlay = getCardIndexInTurnRibbon( effectiveCardsList[random].name );
				playCard( indexOfCardToPlay );
			}
		}
	}

	/// <summary>
	/// Gets the list of cards for which we have enough mana.
	/// </summary>
	/// <returns>The list of playable cards.</returns>
	List<PlayerDeck.PlayerCardData> getListOfPlayableCards()
	{
		List<PlayerDeck.PlayerCardData> playableCardsList = new List<PlayerDeck.PlayerCardData>();
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			if( turnRibbonList[i].manaCost <= manaAmount )
			{
				playableCardsList.Add( getCardByName( turnRibbonList[i].name ) );
			}
		}
		return playableCardsList;
	}
 
	int getCardIndexInTurnRibbon( CardName name )
	{
		return turnRibbonList.FindIndex(cardData => cardData.name == name);
	}
	#endregion

	#region Steal Card
	public CardName stealCard()
	{
		//Pick a random card from the turn ribbon
		int randomCardInTurnRibbon = Random.Range(0, turnRibbonList.Count);
		CardName stolenCardName = turnRibbonList[randomCardInTurnRibbon].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( randomCardInTurnRibbon, stolenCard ) );

		return stolenCardName;
	}

	public void replaceCard( CardName stolenCardName )
	{
		CardManager.CardData stealCard = CardManager.Instance.getCardByName( CardName.Steal );
		int stealCardIndex = turnRibbonList.IndexOf(stealCard);
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );
		turnRibbonList[stealCardIndex] = stolenCard;
		//Also add it to the Bot's battle deck-this will not be saved so it's fine
		PlayerDeck.PlayerCardData stolenPlayerCardData = new PlayerDeck.PlayerCardData();
		stolenPlayerCardData.name = stolenCardName;
		stolenPlayerCardData.level = 3;
		battleDeckList.Add( stolenPlayerCardData );
		Debug.LogWarning("BotCardHandler-replaceCard " + stealCard.name + " by " + stolenCard.name );
	}
	#endregion

	void OnEnable()
	{
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			//When the bot respawns, he usually has enough mana to play a card immediately.
			//We don't want the bot to play as soon as he lands on the ground however because it doesn't feel right.
			//So we reset the time analysis value to half the time it would take before analysing his deck.
			//So for a MEDIUM skilled bot, this means that the bot will wait exactly 2 seconds after respawning before playing a card.
			timeOfLastAnalysis = Time.time - botSkillData.cardPlayFrequency * 0.5f;
		}
	}

}
