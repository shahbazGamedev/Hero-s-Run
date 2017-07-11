using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsUnlockedUI : MonoBehaviour {

	[Header("Top Section")]
	[SerializeField] GameObject closeButton;
	[SerializeField] TextMeshProUGUI sectorNumberText;
	[SerializeField] TextMeshProUGUI sectorNameText;
	[SerializeField] TextMeshProUGUI trophiesNeededText;
	[SerializeField] Image sectorImage;

	[Header("Cards Unlocked")]
	[SerializeField] Transform cardsUnlockedHolder;
	[SerializeField] GameObject cardPrefab;
	[SerializeField] GameObject cardDetailPopup;

	// Use this for initialization
	void Start ()
	{
		createUnlockedCards();
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
		//Simply show the card detail popup
		//It looks better if the close button and sector number text are hidden
		closeButton.SetActive( false );
		sectorNumberText.gameObject.SetActive( false );
		cardDetailPopup.GetComponent<CardUnlockedUI>().configureCard( cd );
		cardDetailPopup.SetActive( true );
	}

	public void OnClickHide()
	{
		gameObject.SetActive( false );
	}
	
}
