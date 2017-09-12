using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed class BotCardHandler : MonoBehaviour {

	HeroManager.BotHeroCharacter botHero;
	HeroManager.BotSkillData botSkillData;
	List<PlayerDeck.PlayerCardData> battleDeckList;
	float powerAmount = PowerBar.START_POWER_POINT;
	CardHandler cardHandler;
	CardManager.CardData nextCard;
	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK);
	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();
	PlayerControl playerControl;
	PlayerSpell playerSpell;
	PlayerRace playerRace;
	bool allowCardPlaying = false;
	float timeOfLastAnalysis = 0;
	#region Game Paused
	bool gamePaused = false;
	float timeRemainingNextAnalysis = 0;
	#endregion

	// Use this for initialization
	void Start ()
	{
		//Get a reference to the CardHandler
		cardHandler = GameObject.FindGameObjectWithTag("Card Handler").GetComponent<CardHandler>();

		botHero = GetComponent<PlayerAI>().botHero;

		//Get and store the bot skill data for that hero.
		botSkillData = HeroManager.Instance.getBotSkillData( botHero.skillLevel );

		//Get and store the battle deck for this hero
		battleDeckList = getBattleDeck();

		//Get and store components
		playerControl = GetComponent<PlayerControl>();
		playerSpell = GetComponent<PlayerSpell>();
		playerRace = GetComponent<PlayerRace>();

		initializeCards ();

		if( botHero.skillLevel != BotSkillLevel.VERY_LOW && GameManager.Instance.playerDebugConfiguration.getAllowBotToPlayCards() ) Invoke("allowBotToPlayCards", botSkillData.raceStartGracePeriod );
	}

	void allowBotToPlayCards()
	{
		 allowCardPlaying = true;
	}

	List<PlayerDeck.PlayerCardData> getBattleDeck()
	{
		//All bot cards belong to BATTLE_DECK_ONE. They have only one deck.
		List<PlayerDeck.PlayerCardData> battleDeck = botHero.botCardDataList.FindAll( card => card.memberOfTheseBattleDecks[(int)BattleDeck.BATTLE_DECK_ONE] == true );
		//Check that we have the right number of cards in the Battle Deck
		if( battleDeck.Count == TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK )
		{
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
		}
		else
		{
			Debug.LogError("The battle deck for " + botHero.userName + " does not contain " +  TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK + " cards. This is set in HeroManager." );
			battleDeck = null;
		}
		if( !battleDeck.Exists( card => card.name == botHero.reservedCard ) )
		{
			Debug.LogError("The battle deck for the bot " + botHero.userName + " and using the hero " + botHero.name + " does not contain the appropriate hero card. It should contain: " + botHero.reservedCard + ". Also make sure the flag inBattleDeck is set to true." );
		}
		return battleDeck;
	}
	
	// Use this for initialization
	void initializeCards ()
	{
		cardIndexList.AddRange(cardIndexArray);

		for( int i = 0; i < CardManager.CARDS_IN_TURN_RIBBON; i++ )
		{
			addCardToTurnRibbon( i, battleDeckList[getUniqueRandom()].name );
		}
		//Set next card with a unique, random card
		setNextCard( battleDeckList[getUniqueRandom()].name );

		//We are queuing 3 cards: 8 - 4 - 1 (card in battle decks - cards in turn ribbon - 1)
		for( int i = 0; i < 3; i++ )
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
		if( gamePaused ) return;

		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			if( powerAmount < PowerBar.MAX_POWER_POINT )
			{
				if( playerRace.isPowerBoostActivated() )
				{
					powerAmount = powerAmount + Time.deltaTime/PowerBar.FAST_POWER_REFILL_RATE;
				}
				else
				{
					powerAmount = powerAmount + Time.deltaTime/PowerBar.DEFAULT_POWER_REFILL_RATE;
				}
			}
			if( allowCardPlaying && (Time.time - timeOfLastAnalysis > botSkillData.cardPlayFrequency) && !playerSpell.isCardActive( CardName.Hack) )
			{
				analyseCards();
			}
		}
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Normal && previousState == GameState.Paused )
		{
			//Unpause
  			gamePaused = false;
			//Recalculate the time of last analysis now that we are unpaused.
			//Why are we doing this? If the player paused for longer than cardPlayFrequency, when the game would be unpaused, the bots would all
			//analyseCards immediately and possibly play cards all at the same time.
			timeOfLastAnalysis = Time.time - timeRemainingNextAnalysis;
		}
		else if( newState == GameState.Paused )
		{
			//Pause
  			gamePaused = true;
			//Measure the time until the next card analysis and save it
			timeRemainingNextAnalysis = ( timeOfLastAnalysis + botSkillData.cardPlayFrequency ) - Time.time;
		}
	}

	void playCard( int indexOfCardToPlay )
	{
		//Which card did the bot select?
		CardName cardName = turnRibbonList[indexOfCardToPlay].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );

		//Deduct the power
		deductPower( playedCard.manaCost );

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
			cardHandler.activateCard( GetComponent<PhotonView>(), cardName, botHero.userName, botCardData.level );
			playerSpell.playedCard( cardName );
		}
	}

	void deductPower( int powerCost )
	{
		if( powerAmount >= powerCost )
		{
			powerAmount = powerAmount - powerCost;
		}
		else
		{
			Debug.LogError("BotCardHandler-deductPower: insufficent power.");
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
	/// If the bot has this card, is allowed to play cards, is not affected by Hack, and has enough power, play the card.
	/// </summary>
	public void tryToPlayCard( CardName cardName )
	{
		if( allowCardPlaying && !playerSpell.isCardActive(CardName.Hack) )
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
	/// Gets the list of cards for which we have enough power.
	/// </summary>
	/// <returns>The list of playable cards.</returns>
	List<PlayerDeck.PlayerCardData> getListOfPlayableCards()
	{
		List<PlayerDeck.PlayerCardData> playableCardsList = new List<PlayerDeck.PlayerCardData>();
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			if( turnRibbonList[i].manaCost <=powerAmount )
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
		GameManager.gameStateEvent += GameStateChange;
	}

	void OnDisable()
	{
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			//When the bot respawns, he usually has enough power to play a card immediately.
			//We don't want the bot to play as soon as he lands on the ground however because it doesn't feel right.
			//So we reset the time analysis value to half the time it would take before analysing his deck.
			//So for a MEDIUM skilled bot, this means that the bot will wait exactly 2 seconds after respawning before playing a card.
			timeOfLastAnalysis = Time.time - botSkillData.cardPlayFrequency * 0.5f;
		}
	}

}
