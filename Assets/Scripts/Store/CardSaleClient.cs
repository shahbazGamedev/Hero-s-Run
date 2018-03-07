using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSaleClient : MonoBehaviour {

	[Header("Card Sale Area")]
	[SerializeField] TextMeshProUGUI timeRemainingBeforeNextCards;
	[SerializeField] Transform cardSaleHolder;
	[SerializeField] GameObject cardOnSalePrefab;
	List<CardName> cardOnSaleList = new List<CardName>();

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

		//cardOnSaleList.Add(CardName.Armor);
		//cardOnSaleList.Add(CardName.Hack);
		//cardOnSaleList.Add(CardName.Health_Boost);
		cardOnSaleList.Add(CardName.Shrink);
		cardOnSaleList.Add(CardName.Shockwave);
		cardOnSaleList.Add(CardName.Freeze);

		createCardsOnSale();

	}

	#region Card Sale
	void createCardsOnSale()
	{
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
		go.GetComponent<CardSaleUI>().configureCard( pcd, cd, 40, 400 );
	}
	#endregion
}
