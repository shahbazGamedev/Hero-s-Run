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
		//Determine the player's current sector.
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getLevelData().getRaceTrackByTrophies();

		//Get all the cards assigned to that sector and display them
		List<CardManager.CardData> allCardsForSectorList = CardManager.Instance.getAllCardsForSector( multiplayerInfo.circuitInfo.sectorNumber );
		for( int i = 0; i < allCardsForSectorList.Count; i++ )
		{
			createUnlockedCard( allCardsForSectorList[i] );
		}
	}

	void createUnlockedCard( CardManager.CardData cd )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardsUnlockedHolder,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveAllListeners();
		cardButton.onClick.AddListener(() => OnClickUnlockedCard( cd ));
		Image cardImage = go.GetComponent<Image>();
		cardImage.sprite = cd.icon;
		//Legendary cards have special effects
		cardImage.material = cd.cardMaterial;

	}

	public void OnClickUnlockedCard( CardManager.CardData cd )
	{
		cardDetailPopup.GetComponent<CardUnlockedUI>().configureCard( cd );
		cardDetailPopup.GetComponent<CardUnlockedUI>().show( true );
	}

}
