using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// Card collection manager.
/// The execution time has been delayed by 200 msec to give time to Card Manager to initialize. See Script Execution Order Settings.
/// </summary>
class CardCollectionManager : MonoBehaviour, IPointerDownHandler {

	[Header("Battle Deck")]
	[SerializeField] Transform battleDeckCardHolder;
	[SerializeField] GameObject cardPrefab;
	[SerializeField] Text battleDeckTitle;
	[SerializeField] Text averageManaCost;
	[Header("Replace Card")]
	[SerializeField] GameObject replaceCardArea;
	[SerializeField] RectTransform cardVerticalContent;
	const float CARD_REPLACEMENT_POSITION = 280f;
	[SerializeField] GameObject cardToAddToBattleDeck;
	CardName cardToAddToBattleDeckName;
	[SerializeField] ScrollRect cardCollectionScollRect;
	bool cardReplacementInProgress = false;
	[Header("Card Collection")]
	[SerializeField] Text cardCollectionTitle;
	[SerializeField] Transform cardCollectionHolder;
	[SerializeField] Text cardFoundText;
	[SerializeField] Text sortCards;
	CardSortMode cardSortMode = CardSortMode.BY_RARITY;
	[Header("Cards to be Found")]
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
			createBattleDeckCard( i, battleDeckList[i] );
		}
		averageManaCost.text = string.Format("Average Mana Cost: {0}", GameManager.Instance.playerDeck.getAverageManaCost().ToString("N1") );
	}

	void createBattleDeckCard( int index, PlayerDeck.PlayerCardData pcd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		CardManager.CardData cd = CardManager.Instance.getCardByName( pcd.name );
		go.transform.SetParent(battleDeckCardHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickBattleCard( go, index, pcd, cd ));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickBattleCard( GameObject go, int index, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		if( cardReplacementInProgress )
		{
			//The player wants to replace the card
			print("player is replacing " + cd.name + " by " + cardToAddToBattleDeckName );
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
		sortCards.text = LocalizationManager.Instance.getText("CARD_SORT_" + cardSortMode.ToString() );
		List<CardManager.CardData> cardDeckList = GameManager.Instance.playerDeck.getCardDeck( cardSortMode );
		for( int i = 0; i < cardDeckList.Count; i++ )
		{
			createCollectionCard( i, cardDeckList[i] );
		}
	}

	void createCollectionCard( int index, CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cd.name );
		go.transform.SetParent(cardCollectionHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickCollectionCard( go, index, pcd, cd ));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickCollectionCard( GameObject go, int index, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardDetailPopup>().configureCard( go, pcd, cd );
		cardDetailPopup.SetActive( true );
	}

	public void OnClickSortCards()
	{
		//Change the sort mode
		cardSortMode = getNextCardSortMode();

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
		int index = 0;
		foreach( CardManager.CardData cardData in cardsNotFoundList )
		{
			createCardToBeFound( index, cardData );
			index++;
		}
	}

	void createCardToBeFound( int index, CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardToBeFoundPrefab);
		go.transform.SetParent(cardsToBeFoundHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveListener(() => OnClickCardToBeFound(index, cd));
		cardButton.onClick.AddListener(() => OnClickCardToBeFound(index, cd));
		Image cardImage = cardButton.GetComponentInChildren<Image>();
		cardImage.sprite = cd.icon;
	}

	public void OnClickCardToBeFound( int index, CardManager.CardData cd )
	{
		//Not implemented
		Debug.Log("OnClickCardToBeFound " + cd.name );
	}
	#endregion

	#region Adding Card to Battle Deck
	public void replaceCard( CardName cardToAddToBattleDeckName )
	{
		this.cardToAddToBattleDeckName = cardToAddToBattleDeckName;
		cardReplacementInProgress = true;
		replaceCardArea.SetActive( true );
		StartCoroutine( scrollToCardReplacementPosition( 0.28f, CARD_REPLACEMENT_POSITION, showCardToAddToBattleDeck ) );
	}
	
	IEnumerator scrollToCardReplacementPosition( float duration, float verticalPosition, System.Action onFinish )
	{
		float elapsedTime = 0;
		Vector2 startVerticalPosition = cardVerticalContent.anchoredPosition;
		Vector2 endVerticalPosition = new Vector2( cardVerticalContent.anchoredPosition.x, verticalPosition );

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			cardVerticalContent.anchoredPosition = Vector2.Lerp( startVerticalPosition, endVerticalPosition, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		cardVerticalContent.anchoredPosition = new Vector2( cardVerticalContent.anchoredPosition.x, verticalPosition );
		onFinish.Invoke();
	}

	void showCardToAddToBattleDeck()
	{
		//Stop vertical scrolling
		cardCollectionScollRect.enabled = false;
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cardToAddToBattleDeckName );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardToAddToBattleDeckName );
		cardToAddToBattleDeck.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		cardToAddToBattleDeck.SetActive( true );
	}

    public void OnPointerDown(PointerEventData data)
    {
		//If we get a pointer down event, it means the player tapped on something other than a card in the battle deck.
		//Assume he changed his mind and no longer wants to replace the card.
		stopCardReplacement();
    }

	void stopCardReplacement()
	{
		cardReplacementInProgress = false;
		cardCollectionScollRect.enabled = true;
		replaceCardArea.SetActive( false );
		cardToAddToBattleDeck.SetActive( false );	
	}
	#endregion

}
