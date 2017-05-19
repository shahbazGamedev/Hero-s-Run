using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTimerHandler : MonoBehaviour {

	void OnEnable()
	{
		TurnRibbonHandler.cardPlayedEvent += CardPlayedEvent;
	}

	void OnDisable()
	{
		TurnRibbonHandler.cardPlayedEvent -= CardPlayedEvent;
	}

	void CardPlayedEvent( CardName name )
	{
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		//Does this card have a DURATION property?
		if( playedCard.doesCardHaveThisProperty( CardPropertyType.DURATION ) )
		{
			string localizedName = LocalizationManager.Instance.getText( "CARD_NAME_" + name.ToString().ToUpper() );
			PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( name );
			float duration = playedCard.getCardPropertyValue( CardPropertyType.DURATION, playerCardData.level );
			addTimer( localizedName, duration );
		}
	}

	void addTimer(string localizedName, float duration )
	{
		print("CardPlayedEvent-addTimer for " + localizedName + " Duration: " + duration );
		//supercharger.startAnimation( localizedName, duration );
	}

}
