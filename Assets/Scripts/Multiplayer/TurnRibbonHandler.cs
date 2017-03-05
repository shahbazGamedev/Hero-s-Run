using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnRibbonHandler : MonoBehaviour {

	[Header("General")]
	[SerializeField] CardHandler cardHandler;
	[SerializeField] Transform cardPanel;
	[SerializeField] GameObject cardPrefab;
	[Header("Next Card")]
	[SerializeField] Image nextCardImage;
	[SerializeField] Text nextCardText;
	CardManager.CardData nextCard;
	[SerializeField] Sprite blankCardSprite;
	[Header("Mana Bar")]
	[SerializeField] ManaBar manaBar;

	const int NUMBER_CARDS_IN_BATTLE_DECK = 8;
	const float DELAY_BEFORE_NEXT_CARD_AVAILABLE = 1f;

	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(NUMBER_CARDS_IN_BATTLE_DECK);

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
			addCardToTurnRibbon( i, battleDeckList[getUniqueRandom()].name );
		}
		//Set next card with a unique, random card
		setNextCard( battleDeckList[getUniqueRandom()].name );

		//We are testing with 3 and with 4 cards in the turn ribbon
		//So we are either queuing 4 cards: 8 - 3 - 1 or 3 cards: 8 - 4 - 1
		int cardsToQueue = NUMBER_CARDS_IN_BATTLE_DECK - CardManager.Instance.cardsInTurnRibbon - 1;
		for( int i = 0; i < cardsToQueue; i++ )
		{
			addCardToQueue( battleDeckList[getUniqueRandom()].name );
		}

	}

	void addCardToTurnRibbon( int index, string cardName )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button cardButton = go.GetComponent<Button>();
		turnRibbonButtonList.Add(cardButton);
		cardButton.onClick.AddListener(() => OnClickCard( index ) );
		cardButton.interactable = false;
		Image cardImage = go.GetComponent<Image>();
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		cardImage.sprite = cardData.icon;
		turnRibbonList.Add(cardData);
	}

	void setNextCard( string cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		nextCardImage.sprite = cardData.icon;
		this.nextCard = cardData;
	}

	void addCardToQueue( string cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		cardQueue.Enqueue( cardData );
	}

	void Update()
	{
		//If we don't have enough mana to play a card, make it non-interactable
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			turnRibbonButtonList[i].interactable = manaBar.hasEnoughMana( turnRibbonList[i].manaCost ) && PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS;
			if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
			{
				float fillAmount = 1f - manaBar.getManaAmount()/turnRibbonList[i].manaCost;
				if( fillAmount < 0 ) fillAmount = 0;
				turnRibbonButtonList[i].transform.FindChild("Radial Mask").GetComponent<Image>().fillAmount = fillAmount;
			}
			else
			{
				turnRibbonButtonList[i].transform.FindChild("Radial Mask").GetComponent<Image>().fillAmount = 1f;
			}
		}
	}

	public void OnClickCard( int indexOfCardPlayed )
	{
		//On which card did the player click?
		string cardName = turnRibbonList[indexOfCardPlayed].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );

		//Deduct the mana
		manaBar.deductMana( playedCard.manaCost );

		//Play the card effect
		playCardEffect( playedCard.name );

		//Temporarily replace the image on the button that was clicked by a blank image
		Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
		buttonOfCardPlayed.GetComponent<Image>().overrideSprite = blankCardSprite;

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardPlayed, playedCard ) );
	}

	IEnumerator moveNextCardIntoTurnRibbon( int indexOfCardPlayed, CardManager.CardData playedCard )
	{
		yield return new WaitForSecondsRealtime( DELAY_BEFORE_NEXT_CARD_AVAILABLE );

		//Replace the image on the button that was clicked by the image of the Next card
		Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
		buttonOfCardPlayed.GetComponent<Image>().overrideSprite = null;
		buttonOfCardPlayed.GetComponent<Image>().sprite = nextCard.icon;

		//In the turn-ribbon list, replace the card played by the card held in Next
		turnRibbonList.RemoveAt(indexOfCardPlayed);
		turnRibbonList.Insert( indexOfCardPlayed, nextCard );

		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		nextCard = oldestCardInQueue;
		nextCardImage.sprite = oldestCardInQueue.icon;

		//Enqueue the card that was just played
		cardQueue.Enqueue( playedCard );
		
	}

	void playCardEffect( string cardName )
	{
		PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( cardName );
		Debug.Log("TurnRibbonHandler-playCardEffect: playing effect for card: " + cardName );
		cardHandler.activateCard( ((GameObject)PhotonNetwork.player.TagObject).GetComponent<PhotonView>().viewID, cardName, playerCardData.level );
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
