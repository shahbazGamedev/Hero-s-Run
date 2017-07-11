using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsUnlockedUI : MonoBehaviour {

	[Header("Cards Unlocked")]
	[SerializeField] Transform cardsUnlockedHolder;
	[SerializeField] GameObject cardPrefab;
	[SerializeField] GameObject cardDetailPopup;

	// Use this for initialization
	void Start ()
	{
		removePreviousCards();
		createUnlockedCards();
	}

	void removePreviousCards()
	{
		//Remove previous cards
		for( int i = cardsUnlockedHolder.childCount-1; i >= 0; i-- )
		{
			Transform child = cardsUnlockedHolder.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}
	}

	void createUnlockedCards()
	{
		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();
		for( int i = 0; i < battleDeckList.Count; i++ )
		{
			createUnlockedCard( battleDeckList[i] );
		}
	}

	void createUnlockedCard( PlayerDeck.PlayerCardData pcd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		CardManager.CardData cd = CardManager.Instance.getCardByName( pcd.name );
		go.transform.SetParent(cardsUnlockedHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickUnlockedCard( cd ));
		Image cardImage = go.GetComponent<Image>();
		cardImage.sprite = cd.icon;
	}

	public void OnClickUnlockedCard( CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardUnlockedUI>().configureCard( cd );
		cardDetailPopup.GetComponent<CardUnlockedUI>().show( true );
	}

}
