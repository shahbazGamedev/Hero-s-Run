using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Card collection manager.
/// The execution time has been delayed by 200 msec to give time to Card Manager to initialize. See Script Execution Order Settings.
/// </summary>
class CardCollectionManager : MonoBehaviour {

	[Header("Battle Deck")]
	[SerializeField] Transform battleDeckCardHolder;
	[Tooltip("TBC.")]
	[SerializeField] GameObject cardPrefab;
	[SerializeField] Text battleDeckTitle;
	[SerializeField] Text averageManaCost;
	[Header("Texts")]
	[SerializeField] GameObject cardDetailPopup;
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
	bool levelLoading = false;
	

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
		cardButton.onClick.RemoveListener(() => OnClickBattleCard(index,pcd, cd));
		cardButton.onClick.AddListener(() => OnClickBattleCard(index,pcd, cd));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickBattleCard( int index, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardDetailPopup>().configureCard( pcd, cd );
		cardDetailPopup.SetActive( true );
		scaleUp();
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
		cardButton.onClick.RemoveListener(() => OnClickCollectionCard(index,pcd, cd));
		cardButton.onClick.AddListener(() => OnClickCollectionCard(index,pcd, cd));
		go.GetComponent<CardUIDetails>().configureCard( pcd, cd );
	}

	public void OnClickCollectionCard( int index, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardDetailPopup>().configureCard( pcd, cd );
		cardDetailPopup.SetActive( true );
		scaleUp();
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

	void scaleUp()
	{
		CancelInvoke("scaleDown");
		LeanTween.cancel( gameObject );
		LeanTween.scale( cardDetailPopup.GetComponent<RectTransform>(), new Vector3( 1.015f, 1.015f, 1.015f ), 0.18f ).setOnComplete(scaleDown).setOnCompleteParam(gameObject);
	}
	
	void scaleDown()
	{
		LeanTween.scale( cardDetailPopup.GetComponent<RectTransform>(), Vector3.one, 0.25f );
	}

	public void OnClickOpenMainMenu()
	{
		//TO DO SAVING
		GameManager.Instance.playerProfile.serializePlayerprofile();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

}
