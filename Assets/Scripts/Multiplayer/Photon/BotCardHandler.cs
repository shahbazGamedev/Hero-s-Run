using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotCardHandler : Photon.PunBehaviour {

	HeroManager.BotHeroCharacter botHero;
	List<PlayerDeck.PlayerCardData> battleDeckList;
	float manaAmount = ManaBar.START_MANA_POINT;
	CardHandler cardHandler;
	CardManager.CardData nextCard;
	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(TurnRibbonHandler.NUMBER_CARDS_IN_BATTLE_DECK);
	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();
	float timeOfLastAnalysis = 0;
	float DELAY_BEFORE_NEXT_ANALYSIS = 2f; //Check which card to play every DELAY_BEFORE_NEXT_ANALYSIS
	PlayerControl playerControl;

	// Use this for initialization
	void Start ()
	{
		//Get a reference to the CardHandler
		cardHandler = GameObject.FindGameObjectWithTag("Card Handler").GetComponent<CardHandler>();

		//Get and store the bot that was selected in MPNetworkLobbyManager and saved in LevelManager.
		botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );
	
		//Get and store the battle deck for this hero
		battleDeckList = getBattleDeck();

		//Get and store PlayerControl
		playerControl = GetComponent<PlayerControl>();

		initializeCards ();

	}

	List<PlayerDeck.PlayerCardData> getBattleDeck()
	{
		List<PlayerDeck.PlayerCardData> battleDeck = botHero.botCardDataList.FindAll( card => card.inBattleDeck == true );
		//Debug.Log("Cards in bot battle deck:\n" );
		for( int i = 0; i < battleDeck.Count; i++ )
		{
			//Debug.Log("Card " + i + " " +  battleDeck[i].name );
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

	void addCardToTurnRibbon( int index, string cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		turnRibbonList.Add(cardData);
	}

	void setNextCard( string cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		nextCard = cardData;
	}

	void addCardToQueue( string cardName )
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
			if( Time.time - timeOfLastAnalysis > DELAY_BEFORE_NEXT_ANALYSIS )
			{
				analyseCards();
			}
		}
	}

	void playCard( int indexOfCardPlayed )
	{
		//Which card did the bot select?
		string cardName = turnRibbonList[indexOfCardPlayed].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );

		//Deduct the mana
		deductMana( playedCard.manaCost );

		//Activate the card
		activateCard( playedCard.name );

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardPlayed, playedCard ) );
	}

	IEnumerator moveNextCardIntoTurnRibbon( int indexOfCardPlayed, CardManager.CardData playedCard )
	{
		yield return new WaitForSecondsRealtime( TurnRibbonHandler.DELAY_BEFORE_NEXT_CARD_AVAILABLE );

		//In the turn-ribbon list, replace the card played by the card held in Next
		turnRibbonList.RemoveAt(indexOfCardPlayed);
		turnRibbonList.Insert( indexOfCardPlayed, nextCard );

		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		nextCard = oldestCardInQueue;

		//Enqueue the card that was just played
		cardQueue.Enqueue( playedCard );

	}

	void activateCard( string cardName )
	{
		PlayerDeck.PlayerCardData botCardData = getCardByName( cardName );
		Debug.Log("BotCardHandler-activateCard: playing card: " + cardName + " level: " + botCardData.level );
		cardHandler.activateCard( this.photonView.viewID, cardName, botCardData.level );
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

	bool doesCardExist( string name )
	{
		return battleDeckList.Exists(cardData => cardData.name == name );
	}

	PlayerDeck.PlayerCardData getCardByName( string name )
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

	#region Card Analysis
	void analyseCards()
	{
		//For the time being, we consider playing a card only in the RUNNING state.
		if( playerControl.getCharacterState() != PlayerCharacterState.Running ) return;

		timeOfLastAnalysis = Time.time;
		//Debug.Log("BotCardHandler-analyseCards" );
		List<CardManager.CardData> playableCardsList = getListOfPlayableCards();

	}

	/// <summary>
	/// Gets the list of cards for which we have enough mana.
	/// </summary>
	/// <returns>The list of playable cards.</returns>
	List<CardManager.CardData> getListOfPlayableCards()
	{
		List<CardManager.CardData> playableCardsList = new List<CardManager.CardData>();
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			if( turnRibbonList[i].manaCost <= manaAmount )
			{
				playableCardsList.Add( turnRibbonList[i] );
			}
		}
		return playableCardsList;
	}
 
	#endregion

}
