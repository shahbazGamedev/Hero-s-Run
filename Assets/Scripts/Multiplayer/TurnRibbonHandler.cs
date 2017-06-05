using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
	[SerializeField] Sprite stolenCardSprite;
	[Header("Mana Bar")]
	[SerializeField] ManaBar manaBar;

	public const int NUMBER_CARDS_IN_BATTLE_DECK = 8;
	public const float DELAY_BEFORE_NEXT_CARD_AVAILABLE = 1f;

	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(NUMBER_CARDS_IN_BATTLE_DECK);

	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	List<Button> turnRibbonButtonList = new List<Button>();
	Queue<CardManager.CardData> cardQueue = new Queue<CardManager.CardData>();
	
	PlayerControl playerControl;
	PlayerSpell playerSpell;

	//Delegate used to communicate to other classes when the local player has played a card
	public delegate void CardPlayedEvent( CardName name, int level );
	public static event CardPlayedEvent cardPlayedEvent;

	// Use this for initialization
	void Start ()
	{
		cardIndexList.AddRange(cardIndexArray);

		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();

		//Populate turn-ribbon with 4 unique, random cards
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

	public void setPlayerControl( PlayerControl playerControl )
	{
		this.playerControl = playerControl;
		playerSpell = playerControl.GetComponent<PlayerSpell>();
	}

	void addCardToTurnRibbon( int index, CardName cardName )
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
		//Card name text and mana cost text
		TextMeshProUGUI[] buttonTexts = cardButton.GetComponentsInChildren<TextMeshProUGUI>();
		buttonTexts[0].text = LocalizationManager.Instance.getText( "CARD_NAME_" + cardData.name.ToString().ToUpper() );
		buttonTexts[1].text = cardData.manaCost.ToString();

		turnRibbonList.Add(cardData);
	}

	void setNextCard( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		nextCardImage.sprite = cardData.icon;
		//Card name text and mana cost text
		TextMeshProUGUI[] texts = nextCardImage.GetComponentsInChildren<TextMeshProUGUI>();
		texts[0].text = LocalizationManager.Instance.getText( "CARD_NAME_" + cardData.name.ToString().ToUpper() );
		texts[1].text = cardData.manaCost.ToString();
		this.nextCard = cardData;
	}

	void addCardToQueue( CardName cardName )
	{
		CardManager.CardData cardData = CardManager.Instance.getCardByName( cardName );
		cardQueue.Enqueue( cardData );
	}

	void LateUpdate()
	{
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			//If the race is not in progress, don't allow the player to play cards.
			//If the player cannot control his character (e.g. because of Stasis), don't allow him to play cards.
			//If the player has been Hacked, don't allow him to play cards.
			Image radialMask = turnRibbonButtonList[i].transform.FindChild("Radial Mask").GetComponent<Image>();
			if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS && playerControl.isPlayerControlEnabled() && !playerSpell.isAffectedByHack() )
			{
				if( manaBar.hasEnoughMana( turnRibbonList[i].manaCost ) )
				{
					//We have enough mana to play this card.
					radialMask.fillAmount = 0;
					turnRibbonButtonList[i].interactable = true;
				}
				else
				{
					//We don't have enough mana to play this card. Make it non-interactable.
					turnRibbonButtonList[i].interactable = false;

					float fillAmount = 1f - manaBar.getManaAmount()/turnRibbonList[i].manaCost;
					if( fillAmount < 0 ) fillAmount = 0;
					radialMask.fillAmount = fillAmount;
				}
			}
			else
			{
				radialMask.fillAmount = 1f;
				turnRibbonButtonList[i].interactable = false;
			}
		}
	}

	public void OnClickCard( int indexOfCardPlayed )
	{
		//On which card did the player click?
		CardName cardName = turnRibbonList[indexOfCardPlayed].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( cardName );

		//Deduct the mana
		manaBar.deductMana( playedCard.manaCost );

		//Play the card effect
		activateCard( playedCard.name );

		//When you play the Steal card, it does not get replaced by the card in the Next slot but by the card
		//you are stealing.
		if( cardName != CardName.Steal )
		{
			//Temporarily replace the image on the button that was clicked by a blank image
			Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
			buttonOfCardPlayed.GetComponent<Image>().overrideSprite = blankCardSprite;
			//Card name text and mana cost text
			TextMeshProUGUI[] buttonTexts = buttonOfCardPlayed.GetComponentsInChildren<TextMeshProUGUI>();
			buttonTexts[0].text = string.Empty;
			buttonTexts[1].text = string.Empty;
	
			//Wait a little before moving the Next Card into the free ribbon slot
			StartCoroutine( moveNextCardIntoTurnRibbon( indexOfCardPlayed, playedCard ) );
	
			//Determine the level of the card since it may be modified by Supercharger.
			PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( cardName );
			int level = playerCardData.level;
			if( playerSpell.isAffectedBySupercharger() )
			{
				int maxLevel = CardManager.Instance.getMaxCardLevelForThisRarity( playedCard.rarity );
				level = Mathf.Min( maxLevel, level + CardSupercharger.SUPERCHARGER_LEVEL_BOOST );
			}

			if( cardPlayedEvent != null ) cardPlayedEvent( cardName, level );
		}
	}

	IEnumerator moveNextCardIntoTurnRibbon( int indexOfCardPlayed, CardManager.CardData playedCard )
	{
		yield return new WaitForSecondsRealtime( DELAY_BEFORE_NEXT_CARD_AVAILABLE );

		//Replace the image on the button that was clicked by the image of the Next card
		Button buttonOfCardPlayed = turnRibbonButtonList[indexOfCardPlayed];
		buttonOfCardPlayed.GetComponent<Image>().overrideSprite = null;
		buttonOfCardPlayed.GetComponent<Image>().sprite = nextCard.icon;
		//Card name text and mana cost text
		TextMeshProUGUI[] buttonTexts = buttonOfCardPlayed.GetComponentsInChildren<TextMeshProUGUI>();
		buttonTexts[0].text = LocalizationManager.Instance.getText( "CARD_NAME_" + nextCard.name.ToString().ToUpper() );
		buttonTexts[1].text = nextCard.manaCost.ToString();
		buttonTexts[2].text = string.Empty;

		//In the turn-ribbon list, replace the card played by the card held in Next
		turnRibbonList.RemoveAt(indexOfCardPlayed);
		turnRibbonList.Insert( indexOfCardPlayed, nextCard );

		//Dequeue the oldest card and place it in Next
		CardManager.CardData oldestCardInQueue = cardQueue.Dequeue();
		setNextCard( oldestCardInQueue.name );

		//Enqueue the card that was just played
		cardQueue.Enqueue( playedCard );
		
	}

	void activateCard( CardName cardName )
	{
		PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( cardName );
		Debug.Log("TurnRibbonHandler-activateCard: playing card: " + cardName );
		HeroManager.HeroCharacter selectedHero = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex );
		cardHandler.activateCard( playerControl.GetComponent<PhotonView>(), cardName, selectedHero.name, playerCardData.level );
		//Increase the card usage count. This is used to determine the player's favorite card.
		playerCardData.timesUsed++;
	}

	#region Steal Card
	public CardName stealCard()
	{
		//Pick a random card from the turn ribbon
		int randomCardInTurnRibbon = Random.Range(0, turnRibbonList.Count);
		CardName stolenCardName = turnRibbonList[randomCardInTurnRibbon].name;

		//Get data about the card - make sure NOT to modify the card data
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );
		//Temporarily replace the image on the button that was clicked by a blank image
		Button buttonOfCardPlayed = turnRibbonButtonList[randomCardInTurnRibbon];
		buttonOfCardPlayed.GetComponent<Image>().overrideSprite = stolenCardSprite;
		//Card name text and mana cost text
		TextMeshProUGUI[] buttonTexts = buttonOfCardPlayed.GetComponentsInChildren<TextMeshProUGUI>();
		buttonTexts[0].text = string.Empty;
		buttonTexts[1].text = string.Empty;
		buttonTexts[2].text = LocalizationManager.Instance.getText("CARD_STOLEN");

		//Wait a little before moving the Next Card into the free ribbon slot
		StartCoroutine( moveNextCardIntoTurnRibbon( randomCardInTurnRibbon, stolenCard ) );

		return stolenCardName;
	}

	public void replaceCard( CardName stolenCardName )
	{
		CardManager.CardData stealCard = CardManager.Instance.getCardByName( CardName.Steal );
		int stealCardIndex = turnRibbonList.IndexOf(stealCard);
		CardManager.CardData stolenCard = CardManager.Instance.getCardByName( stolenCardName );
		stolenCard.isStolenCard = true;
		turnRibbonList[stealCardIndex] = stolenCard;
		Debug.LogWarning("TurnRibbonHandler-replaceCard " + stealCard.name + " by " + stolenCardName );

		Button buttonOfCardPlayed = turnRibbonButtonList[stealCardIndex];
		buttonOfCardPlayed.GetComponent<Image>().overrideSprite = null;
		buttonOfCardPlayed.GetComponent<Image>().sprite = stolenCard.icon;
		//Card name text and mana cost text
		TextMeshProUGUI[] buttonTexts = buttonOfCardPlayed.GetComponentsInChildren<TextMeshProUGUI>();
		buttonTexts[0].text = LocalizationManager.Instance.getText( "CARD_NAME_" + stolenCard.name.ToString().ToUpper() );
		buttonTexts[1].text = stolenCard.manaCost.ToString();

	}
	#endregion

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
