using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSaleClient : MonoBehaviour {

	[Header("Content")]
	[SerializeField] RectTransform contentRectTransfom;
	[SerializeField] LayoutElement topImageLayoutElement;
	[Header("Card Sale Area")]
	[SerializeField] LayoutElement cardSaleLayoutElement;
	[SerializeField] Transform cardSaleHolder;
	[SerializeField] GameObject cardOnSalePrefab;

	void Start ()
	{
		initialize ();
	}

	// Use this for initialization
	public void initialize ()
	{
		//Remove previous cards in all 3 sections since this method can get called multiple times (for example
		//if a player finds a brand new card in a loot box).
		for( int i = cardSaleHolder.childCount-1; i >= 0; i-- )
		{
			Transform child = cardSaleHolder.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		createCardsOnSale();

		//Adjust the overall length of the content including all areas
		//float totalLength = topImageLayoutElement.minHeight + battleDeckLayoutElement.minHeight + + heroCardsLayoutElement.minHeight + cardCollectionLayoutElement.minHeight + cardsToBeFoundLayoutElement.minHeight;
		//contentRectTransfom.sizeDelta = new Vector2( contentRectTransfom.sizeDelta.x, totalLength );
	}

	#region Card Sale
	void createCardsOnSale()
	{
		List<CardName> cardOnSaleList = new List<CardName>();
		cardOnSaleList.Add(CardName.Armor);
		cardOnSaleList.Add(CardName.Hack);
		cardOnSaleList.Add(CardName.Health_Boost);
		cardOnSaleList.Add(CardName.Shrink);
		cardOnSaleList.Add(CardName.Shockwave);
		cardOnSaleList.Add(CardName.Smoke_Bomb);

		for( int i = 0; i < cardOnSaleList.Count; i++ )
		{
			createCardOnSale( cardOnSaleList[i] );
		}
	}

	void createCardOnSale( CardName cardName )
	{
		PlayerDeck.PlayerCardData pcd = GameManager.Instance.playerDeck.getCardByName( cardName );
		CardManager.CardData cd = CardManager.Instance.getCardByName( pcd.name );
		GameObject go = (GameObject)Instantiate(cardOnSalePrefab);
		go.transform.SetParent(cardSaleHolder,false);

		TextMeshProUGUI[]texts = go.GetComponentsInChildren<TextMeshProUGUI>();
		//Card name
		texts[0].text = LocalizationManager.Instance.getText( "CARD_NAME_" + pcd.name.ToString().ToUpper() );
		//Card rarity
		texts[1].text = LocalizationManager.Instance.getText( "CARD_RARITY_" + cd.rarity.ToString().ToUpper() );
		//Listener
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickCardOnSale( go, pcd, cd ));
		//Card image and progress bar
		Transform card = go.transform.FindChild("Card");
		card.GetComponent<CardUIDetails>().configureCard( pcd, cd );
		//Cost
		Transform costHolder = go.transform.FindChild("Cost Holder");
		int costToPurchaseOneCard = 2;
		costHolder.GetComponentInChildren<TextMeshProUGUI>().text = costToPurchaseOneCard.ToString("N0");
	}

	public void OnClickCardOnSale( GameObject go, PlayerDeck.PlayerCardData pcd, CardManager.CardData cd )
	{
		print("OnClickCardOnSale " + pcd.name );
	}

	#endregion
}
