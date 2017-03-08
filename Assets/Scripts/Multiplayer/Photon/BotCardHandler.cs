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
	float DELAY_BEFORE_NEXT_ANALYSIS = 3f; //Check which card to play every DELAY_BEFORE_NEXT_ANALYSIS
	PlayerControl playerControl;
	float MINIMUM_EFFECTIVENESS = 0.2f;
	bool allowCardPlaying = false;
	const float START_OF_RACE_GRACE_PERIOD = 15f;

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

		Invoke("allowBotToPlayCards", START_OF_RACE_GRACE_PERIOD );
	}

	void allowBotToPlayCards()
	{
		 allowCardPlaying = true;
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
			if( allowCardPlaying && (Time.time - timeOfLastAnalysis > DELAY_BEFORE_NEXT_ANALYSIS) )
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

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardToPlay, playedCard ) );
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

	void activateCard( CardName cardName )
	{
		PlayerDeck.PlayerCardData botCardData = getCardByName( cardName );
		Debug.LogWarning("BotCardHandler-activateCard: playing card: " + cardName + " level: " + botCardData.level + " " + this.photonView.viewID );
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

	#region Card Analysis
	void analyseCards()
	{
		//For the time being, we consider playing a card only in the RUNNING state.
		if( playerControl.getCharacterState() != PlayerCharacterState.Running ) return;

		timeOfLastAnalysis = Time.time;
		//Debug.Log("BotCardHandler-analyseCards" );
		List<CardManager.CardData> playableCardsList = getListOfPlayableCards();

		//Do we have at least one playable card?
		if( playableCardsList.Count > 0 )
		{
			//Calculate the distance between the bot and the player
			GameObject player = ((GameObject)PhotonNetwork.player.TagObject);
			float botPlayerDistance = Vector3.Distance( transform.position, player.transform.position );

			//Who is leading the race?
			int botRacePosition = GetComponent<PlayerRace>().racePosition;
			int playerRacePosition = player.GetComponent<PlayerRace>().racePosition;
			bool isBotLeading = (botRacePosition < playerRacePosition);
	
			float mostEffectiveCardStrength = 0;
			CardManager.CardData mostEffectiveCard = null;
	
			//Find the most effective card if any
			for( int i = 0; i < playableCardsList.Count; i++ )
			{
				float cardEffectiveness = 0;
				for( int j = 0; j < playableCardsList[i].cardDataRuleList.Count; j++ )
				{
					float ruleEffectiveness = calculateRuleEffectiveness ( playableCardsList[i].cardDataRuleList[j].carRule, botPlayerDistance, isBotLeading ) * playableCardsList[i].cardDataRuleList[j].weight;
					cardEffectiveness = cardEffectiveness + ruleEffectiveness;
				}
				if( cardEffectiveness > MINIMUM_EFFECTIVENESS )
				{
					if( cardEffectiveness > mostEffectiveCardStrength )
					{
						mostEffectiveCardStrength = cardEffectiveness;
						mostEffectiveCard = playableCardsList[i];
					}
				}
			}
	
			//If we found a card, play it
			if( mostEffectiveCard != null )
			{
				int indexOfCardToPlay = getCardIndexInTurnRibbon( mostEffectiveCard.name );
				playCard( indexOfCardToPlay );
			}
		}
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
 
	/// <summary>
	/// Calculates the rule effectiveness. The value should be between 0 (ineffective) and 1 (very effective).
	/// </summary>
	/// <returns>The rule effectiveness.</returns>
	/// <param name="rule">Rule.</param>
	/// <param name="botPlayerDistance">Bot player distance.</param>
	float calculateRuleEffectiveness ( CardRule rule, float botPlayerDistance, bool isBotLeading )
	{
		Debug.LogWarning("BotCardHandler-calculateRuleEffectiveness Rule: " + rule + " distance " + botPlayerDistance + " isBotLeading " + isBotLeading );
		float ruleEffectiveness = 0;
		switch (rule)
		{
			case CardRule.OPPONENT_FAR_LEADING:
				if( botPlayerDistance > 10f && !isBotLeading ) ruleEffectiveness = 1;
			break;
			case CardRule.OPPONENT_FAR_TRAILING:
				if( botPlayerDistance > 10f && isBotLeading ) ruleEffectiveness = 1;
			break;
			case CardRule.OPPONENT_NEAR_LEADING:
				if( botPlayerDistance < 5f && !isBotLeading ) ruleEffectiveness = 1;
			break;
			case CardRule.OPPONENT_NEAR_TRAILING:
				if( botPlayerDistance < 5f && isBotLeading ) ruleEffectiveness = 1;
			break;
			case CardRule.OPPONENT_NEAR:
				if( botPlayerDistance < 5f ) ruleEffectiveness = 1;
			break;
			case CardRule.MANA_COST:
				Debug.LogWarning("BotCardHandler-calculateRuleEffectiveness: MANA_COST rule not implemented.");
			break;
			case CardRule.NO_OBSTACLES_IN_FRONT:
				Debug.LogWarning("BotCardHandler-calculateRuleEffectiveness: NO_OBSTACLES_IN_FRONT rule not implemented.");
			break;
			default:
			break;
		}
		return ruleEffectiveness;
	}

	int getCardIndexInTurnRibbon( CardName name )
	{
		return turnRibbonList.FindIndex(cardData => cardData.name == name);
	}


	#endregion

}
