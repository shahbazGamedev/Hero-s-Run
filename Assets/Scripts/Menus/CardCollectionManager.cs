using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Card collection manager.
/// The execution time has been delayed by 200 msec to give time to Card Manager to initialize. See Script Execution Order Settings.
/// </summary>
class CardCollectionManager : MonoBehaviour, IPointerDownHandler {

	[Header("Battle Deck Area")]
	[SerializeField] Transform battleDeckCardHolder;
	[SerializeField] GameObject cardPrefab;
	[SerializeField] Text battleDeckTitle;
	[SerializeField] Text averageManaCost;
	[Header("Replace Card Area")]
	[SerializeField] GameObject replaceCardArea;
	[SerializeField] RectTransform cardVerticalContent;
	const float CARD_REPLACEMENT_POSITION = 280f;
	[SerializeField] GameObject cardToAddToBattleDeck;
	CardName cardToAddToBattleDeckName;
	[SerializeField] ScrollRect cardCollectionScollRect;
	[SerializeField] ScrollRect horizontalScrollview;
	bool cardReplacementInProgress = false;
	GameObject cardInCollection;
	[Header("Card Collection Area")]
	[SerializeField] LayoutElement cardCollectionLayoutElement;
	[SerializeField] Text cardCollectionTitle;
	[SerializeField] Transform cardCollectionHolder;
	[SerializeField] Text cardFoundText;
	[SerializeField] Text sortCardsText;
	CardSortMode cardSortMode = CardSortMode.BY_RARITY;
	[Header("Cards to be Found Area")]
	[SerializeField] LayoutElement cardsToBeFoundLayoutElement;
	[SerializeField] Text cardsToBeFoundTitle;
	[SerializeField] Transform cardsToBeFoundHolder;
	[SerializeField] GameObject cardToBeFoundPrefab;
	[Header("Card Details Popup")]
	[SerializeField] GameObject cardDetailPopup;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		createBattleDeck();
		createCardCollection();
		createCardsToBeFoundSection();
	}

	#region Battle Deck
	void createBattleDeck()
	{
		battleDeckTitle.text = LocalizationManager.Instance.getText("CARD_BATTLE_DECK_TITLE");
		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();
		for( int i = 0; i < battleDeckList.Count; i++ )
		{
			createBattleDeckCard( battleDeckList[i] );
		}
		averageManaCost.text = string.Format("Average Mana Cost: {0}", GameManager.Instance.playerDeck.getAverageManaCost().ToString("N1") );
	}

	void createBattleDeckCard( PlayerDeck.PlayerCardData pcd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		CardManager.CardData cd = CardManager.Instance.getCardByName( pcd.name );
		go.transform.SetParent(battleDeckCardHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickBattleCard( go, pcd, cd ));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickBattleCard( GameObject go, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		if( cardReplacementInProgress )
		{
			//The player wants to replace the card
			print("player is replacing " + cd.name + " by " + cardToAddToBattleDeckName );

			//Mark card added as being in battle deck
			GameManager.Instance.playerDeck.changeInBattleDeckStatus( cardToAddToBattleDeckName, true );

			//Remove card replaced from being in battle deck
			GameManager.Instance.playerDeck.changeInBattleDeckStatus( cd.name, false );

			//Recalculate the average mana cost
			averageManaCost.text = string.Format("Average Mana Cost: {0}", GameManager.Instance.playerDeck.getAverageManaCost().ToString("N1") );

			//Re-sort the cards in the card collection section
			sortCards();

			//In the battle deck section, update the UI with the card details
			PlayerDeck.PlayerCardData cardAddedPlayer = GameManager.Instance.playerDeck.getCardByName( cardToAddToBattleDeckName );
			CardManager.CardData cardAdded = CardManager.Instance.getCardByName( cardToAddToBattleDeckName );
			Button cardAddedButton = go.GetComponent<Button>();
			cardAddedButton.onClick.RemoveAllListeners();
			cardAddedButton.onClick.AddListener(() => OnClickBattleCard( go, cardAddedPlayer, cardAdded ));
			go.GetComponent<CardUIDetails>().configureCard( cardAddedPlayer, cardAdded );

			//In the card collection section, update the UI with the card details
			PlayerDeck.PlayerCardData cardReplacedPlayer = GameManager.Instance.playerDeck.getCardByName( cd.name );
			CardManager.CardData cardReplaced = CardManager.Instance.getCardByName( cd.name );
			Button cardReplacedButton = cardInCollection.GetComponent<Button>();
			cardReplacedButton.onClick.RemoveAllListeners();
			cardReplacedButton.onClick.AddListener(() => OnClickCollectionCard( cardInCollection, cardReplacedPlayer, cardReplaced ));
			cardInCollection.GetComponent<CardUIDetails>().configureCard( cardReplacedPlayer, cardReplaced );

			//Save card collection
			GameManager.Instance.playerDeck.serializePlayerDeck( true );
	
			//Stop the card replacement mode
			stopCardReplacement();
		}
		else
		{
			//Simply show the card details popup
			cardDetailPopup.GetComponent<CardDetailPopup>().configureCard( go, pcd, cd );
			cardDetailPopup.SetActive( true );
		}
	}
	#endregion

	#region Card Collection
	void createCardCollection()
	{
		cardCollectionTitle.text = LocalizationManager.Instance.getText("CARD_COLLECTION_TITLE");
		string cardFound = LocalizationManager.Instance.getText("CARD_FOUND");
		cardFoundText.text = string.Format( cardFound, GameManager.Instance.playerDeck.getTotalNumberOfCards().ToString() + "/" + CardManager.Instance.getTotalNumberOfCards().ToString() );
		sortCardsText.text = LocalizationManager.Instance.getText("CARD_SORT_" + cardSortMode.ToString() );
		List<CardManager.CardData> cardDeckList = GameManager.Instance.playerDeck.getCardDeck( cardSortMode );
		for( int i = 0; i < cardDeckList.Count; i++ )
		{
			createCollectionCard( cardDeckList[i] );
		}

		//Now adjust the height of the area based on the number of card rows. We have 4 cards per row. We also have a title.
		float titleHeight = cardCollectionTitle.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
		int numberOfRows = (int) Math.Ceiling( (cardDeckList.Count)/4f );
		float cardCollectionAreaHeight = titleHeight + cardCollectionHolder.GetComponent<GridLayoutGroup>().cellSize.y * numberOfRows;
		cardCollectionLayoutElement.preferredHeight = cardCollectionAreaHeight;
		cardCollectionLayoutElement.minHeight = cardCollectionAreaHeight;
	}

	void createCollectionCard( CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cd.name );
		go.transform.SetParent(cardCollectionHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickCollectionCard( go, pcd, cd ));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickCollectionCard( GameObject go, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardDetailPopup>().configureCard( go, pcd, cd );
		cardDetailPopup.SetActive( true );
	}

	public void OnClickSortCards()
	{
		//Change the sort mode
		cardSortMode = getNextCardSortMode();

		sortCards();
	}

	void sortCards()
	{
		//Remove previous cards
		for( int i = cardCollectionHolder.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = cardCollectionHolder.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		//Re-create card collection
		createCardCollection();
	}

	CardSortMode getNextCardSortMode()
	{
		if( cardSortMode == CardSortMode.BY_MANA_COST )
		{
			return CardSortMode.BY_RARITY;
		}
		else
		{
			return CardSortMode.BY_MANA_COST;
		}
	}
	#endregion

	#region Cards to be Found
	void createCardsToBeFoundSection()
	{
		cardsToBeFoundTitle.text = LocalizationManager.Instance.getText("CARD_TO_BE_FOUND_TITLE");
		var cardsNotFoundList = CardManager.Instance.getAllCards().Except( GameManager.Instance.playerDeck.getPlayerCardDeck() );
		foreach( CardManager.CardData cardData in cardsNotFoundList )
		{
			createCardToBeFound( cardData );
		}

		//Now adjust the height of the area based on the number of card rows. We have 4 cards per row. We also have a title.
		float titleHeight = cardsToBeFoundTitle.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
		int numberOfRows = (int) Math.Ceiling( cardsNotFoundList.Count()/4f );
		float cardsToBeFoundAreaHeight = titleHeight + cardsToBeFoundHolder.GetComponent<GridLayoutGroup>().cellSize.y * numberOfRows;
		cardsToBeFoundLayoutElement.preferredHeight = cardsToBeFoundAreaHeight;
		cardsToBeFoundLayoutElement.minHeight = cardsToBeFoundAreaHeight;
	}

	void createCardToBeFound( CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardToBeFoundPrefab);
		go.transform.SetParent(cardsToBeFoundHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveListener(() => OnClickCardToBeFound(cd));
		cardButton.onClick.AddListener(() => OnClickCardToBeFound(cd));
		Image cardImage = cardButton.GetComponentInChildren<Image>();
		cardImage.sprite = cd.icon;
	}

	public void OnClickCardToBeFound( CardManager.CardData cd )
	{
		//Not implemented
		Debug.Log("OnClickCardToBeFound " + cd.name );
	}
	#endregion

	#region Adding Card to Battle Deck
	public void replaceCard( GameObject cardInCollection, CardName cardToAddToBattleDeckName )
	{
		StartCoroutine( cardReplace( cardInCollection, cardToAddToBattleDeckName ) );
	}

	IEnumerator cardReplace( GameObject cardInCollection, CardName cardToAddToBattleDeckName )
	{
		this.cardInCollection = cardInCollection;
		this.cardToAddToBattleDeckName = cardToAddToBattleDeckName;
		cardReplacementInProgress = true;
		replaceCardArea.SetActive( true );
		cardVerticalContent.anchoredPosition = new Vector2( cardVerticalContent.anchoredPosition.x, CARD_REPLACEMENT_POSITION );
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cardToAddToBattleDeckName );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardToAddToBattleDeckName );
		cardToAddToBattleDeck.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		cardToAddToBattleDeck.SetActive( true );
		//If we don't wait for the end of frame to disable the scrolling, the vertical layout will not expand to show
		//the replace card area.
		yield return new WaitForEndOfFrame();  
		//Stop vertical scrolling
		cardCollectionScollRect.enabled = false;
	}
	
    public void OnPointerDown(PointerEventData data)
    {
		//If we get a pointer down event, it means the player tapped on something other than a card in the battle deck.
		//Assume he changed his mind and no longer wants to replace the card.
		stopCardReplacement();
    }

	void stopCardReplacement()
	{
		if( cardReplacementInProgress )
		{
			cardReplacementInProgress = false;
			cardCollectionScollRect.enabled = true;
			replaceCardArea.SetActive( false );
			cardToAddToBattleDeck.SetActive( false );	
			//Re-enable scrolling
			horizontalScrollview.enabled = true;
		}
	}
	#endregion

}
