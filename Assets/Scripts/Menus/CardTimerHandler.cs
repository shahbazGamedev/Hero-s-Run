﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTimerHandler : MonoBehaviour {

	[SerializeField] GameObject radialTimerPrefab;
	[SerializeField] Transform timerHolder;
	List<GameObject> timerList = new List<GameObject>();

	void OnEnable()
	{
		TurnRibbonHandler.cardPlayedEvent += CardPlayedEvent;
		PlayerSpell.cardPlayedByOpponentEvent += CardPlayedByOpponentEvent;
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		TurnRibbonHandler.cardPlayedEvent -= CardPlayedEvent;
		PlayerSpell.cardPlayedByOpponentEvent -= CardPlayedByOpponentEvent;
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	/// <summary>
	/// A card was played by the local player.
	/// Timers will only be added for Cards with a DURATION_WITH_TIMER property.
	/// </summary>
	/// <param name="name">Name.</param>
	void CardPlayedEvent( CardName name )
	{
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		//Does this card have a DURATION_WITH_TIMER property?
		if( playedCard.doesCardHaveThisProperty( CardPropertyType.DURATION_WITH_TIMER ) )
		{
			string localizedName = LocalizationManager.Instance.getText( "CARD_NAME_" + name.ToString().ToUpper() );
			PlayerDeck.PlayerCardData playerCardData = GameManager.Instance.playerDeck.getCardByName( name );
			float duration = playedCard.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, playerCardData.level );
			addTimer( localizedName, duration, Color.green );
		}
	}

	/// <summary>
	/// A Card was played by an opponent.
	/// Only some cards will cause a timer to be created such as Linked Fate, Shrink and Hack.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="duration">Duration.</param>
	void CardPlayedByOpponentEvent( CardName name, float duration )
	{
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		string localizedName = LocalizationManager.Instance.getText( "CARD_NAME_" + name.ToString().ToUpper() );
		addTimer( localizedName, duration, Color.red );
	}

	void addTimer(string localizedName, float duration, Color color )
	{
		GameObject go = (GameObject)Instantiate(radialTimerPrefab);
		go.transform.SetParent(timerHolder,false);
		timerList.Add( go );
		go.GetComponent<RadialTimerUI>().startAnimation( localizedName, duration, color );
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			removeAllTimers();
		}
	}

	/// <summary>
	/// Will Remove all timers. Called when the player dies.
	/// </summary>
	void removeAllTimers()
	{
		for( int i = timerHolder.transform.childCount-1; i >= 0; i-- )
		{
			Transform child = timerHolder.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}
		timerList.Clear();
	}

}
