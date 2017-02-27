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

	// Use this for initialization
	void Start ()
	{
		//Newly unlocked icons appear first.
		List<PlayerDeck.PlayerCardData> battleDeckList = GameManager.Instance.playerDeck.getBattleDeck();

		for( int i = 0; i < 4; i++ )
		{
			addCardToTurnRibbon(battleDeckList[i]);
		}
	}

	void addCardToTurnRibbon( PlayerDeck.PlayerCardData card )
	{
		GameObject go = (GameObject)Instantiate(cardPrefab);
		go.transform.SetParent(cardPanel,false);
		Button cardButton = go.GetComponent<Button>();
		cardButton.onClick.RemoveListener(() => OnClickPlayerIcon(card));
		cardButton.onClick.AddListener(() => OnClickPlayerIcon(card));
		Image cardImage = go.GetComponent<Image>();
		CardManager.CardData cardData = CardManager.Instance.getCardByName( card.name );
		cardImage.sprite = cardData.icon;
		card.rectTransform = go.GetComponent<RectTransform>();
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

}
