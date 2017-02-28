using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnRibbonHandler : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform cardPanel;
	[SerializeField] GameObject cardPrefab;
	[Header("Next Card")]
	[SerializeField] Image nextCardImage;
	[SerializeField] Text nextCardText;
	CardManager.CardData nextCard;
	[Header("Mana Bar")]
	[SerializeField] ManaBar manaBar;

	const int NUMBER_CARDS_IN_BATTLE_DECK = 8;

	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(8);
	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	List<Button> turnRibbonButtonList = new List<Button>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();

	// Use this for initialization
	void Start ()
	{
		cardIndexList.AddRange(cardIndexArray);

		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();

		//Populate turn-ribbon with 3 or 4 unique, random cards
		for( int i = 0; i < CardManager.Instance.cardsInTurnRibbon; i++ )
		{
			addCardToTurnRibbon(battleDeckList[getUniqueRandom()]);
		}
		//Set next card with a unique, random card
		setNextCard( battleDeckList[getUniqueRandom()] );

		//We are testing with 3 and with 4 cards in the turn ribbon
		//So we are either queuing 4 cards: 8 - 3 - 1 or 3 cards: 8 - 4 - 1
		int cardsToQueue = NUMBER_CARDS_IN_BATTLE_DECK - CardManager.Instance.cardsInTurnRibbon - 1;
		for( int i = 0; i < cardsToQueue; i++ )
		{
			addCardToQueue(battleDeckList[getUniqueRandom()]);
		}

	}

	void addCardToTurnRibbon( PlayerDeck.PlayerCardData card )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button cardButton = go.GetComponent<Button>();
		turnRibbonButtonList.Add(cardButton);
		cardButton.onClick.RemoveListener(() => OnClickCard(card));
		cardButton.onClick.AddListener(() => OnClickCard(card));
		Image cardImage = go.GetComponent<Image>();
		CardManager.CardData cardData = CardManager.Instance.getCardByName( card.name );
		cardImage.sprite = cardData.icon;
		card.rectTransform = go.GetComponent<RectTransform>();
		turnRibbonList.Add(cardData);

	}

	void setNextCard( PlayerDeck.PlayerCardData nextCard )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( nextCard.name );
		nextCardImage.sprite = cardData.icon;
		this.nextCard = cardData;
	}

	void addCardToQueue( PlayerDeck.PlayerCardData cardToQueue )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardToQueue.name );
		cardQueue.Enqueue( cardData );
	}

	void Update()
	{
		//If we don't have enough mana to play a card, make it non-interactable
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			turnRibbonButtonList[i].interactable = manaBar.hasEnoughMana( turnRibbonList[i].manaCost );
		}
	}

	public void OnClickCard( PlayerDeck.PlayerCardData card )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( card.name );

		manaBar.deductMana( cardData.manaCost );

		//Play the card effect
		playCardEffect( cardData.name );
		//Replace the card played by the card held in Next
		int indexOfCardPlayed = turnRibbonList.FindIndex( element => element == cardData );
		Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
		buttonOfCardPlayed.GetComponent<Image>().sprite = nextCard.icon;
		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		nextCard = oldestCardInQueue;
		nextCardImage.sprite = oldestCardInQueue.icon;
		//Enqueue the card that was just played
		cardQueue.Enqueue( cardData );
	}

	void playCardEffect( string cardName )
	{
		//To be implemented
		Debug.Log("TurnRibbonHandler-playCardEffect: playing effect for card: " + cardName );
	}

	int getUniqueRandom()
	{
		if(cardIndexList.Count == 0 )
		{
			Debug.LogError("TurnRibbonHandler-getUniqueRandom: cardIndexList is empty.");
			return -1;
		}
		int rand = Random.Range(0, cardIndexList.Count);
		int value = cardIndexList[rand];
		cardIndexList.RemoveAt(rand);
		return value;
	}
}
