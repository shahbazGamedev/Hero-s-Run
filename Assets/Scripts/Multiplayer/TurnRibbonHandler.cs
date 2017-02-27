using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnRibbonHandler : MonoBehaviour {

	[Header("General")]
	[SerializeField] Transform cardPanel;
	[SerializeField] GameObject cardPrefab;
	[Header("On Select")]
	[SerializeField] GameObject onSelectButton;
	[SerializeField] Image onSelectPlayerIcon;
	[Header("Next Card")]
	[SerializeField] Image nextCardImage;
	[SerializeField] Text nextCardText;
	[Header("Mana Bar")]
	[SerializeField] ManaBar manaBar;

	int [] cardIndexArray = new int[]{0,1,2,3,4,5,6,7};
	List<int> cardIndexList = new List<int>(8);
	List<CardManager.CardData> turnRibbonList = new List<CardManager.CardData>();
	List<Button> turnRibbonButtonList = new List<Button>();
	CardManager.CardData nextCard;

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
	}

	void addCardToTurnRibbon( PlayerDeck.PlayerCardData card )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button cardButton = go.GetComponent<Button>();
		turnRibbonButtonList.Add(cardButton);
		cardButton.onClick.RemoveListener(() => OnClickPlayerIcon(card));
		cardButton.onClick.AddListener(() => OnClickPlayerIcon(card));
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

	void Update()
	{
		//If we don't have enough mana to play a card, make it non-interactable
		for( int i = 0; i < turnRibbonList.Count; i++ )
		{
			turnRibbonButtonList[i].interactable = manaBar.hasEnoughMana( turnRibbonList[i].manaCost );
		}
	}

	public void OnClickPlayerIcon( PlayerDeck.PlayerCardData card )
	{
		print("OnClickPlayerIcon");
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		onSelectRectTransform.localScale = Vector3.one;

		//Position on select game object on top of the selected entry
		onSelectButton.transform.SetParent( card.rectTransform, false );
		onSelectButton.SetActive( true );

		//Copy the icon and name
		CardManager.CardData cardData = CardManager.Instance.getCardByName( card.name );
		onSelectPlayerIcon.sprite = cardData.icon;

		scaleUp();

		manaBar.deductMana( cardData.manaCost );

	}

	void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, new Vector3( 1.2f, 1.2f, 1.2f ), 0.18f ).setEaseOutQuad();
		Invoke( "scaleDown", 0.36f );
	}
	
	void scaleDown()
	{
		//Make it the normal size after a few seconds
		RectTransform onSelectRectTransform = onSelectButton.GetComponent<RectTransform>();
		LeanTween.scale( onSelectRectTransform, Vector3.one, 0.18f ).setOnComplete(hide).setOnCompleteParam(gameObject).setEaseOutQuad();
	}

	void hide()
	{
		onSelectButton.SetActive( false );
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
