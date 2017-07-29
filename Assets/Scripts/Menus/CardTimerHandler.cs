using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTimerHandler : MonoBehaviour {

	[SerializeField] GameObject radialTimerPrefab;
	[SerializeField] Transform timerHolder;
	List<CardTimerData> cardTimerDataList = new List<CardTimerData>();

	void OnEnable()
	{
		TurnRibbonHandler.cardPlayedEvent += CardPlayedEvent;
		PlayerSpell.cardPlayedByOpponentEvent += CardPlayedByOpponentEvent;
		PlayerSpell.cardCanceledEvent += CardCanceledEvent;
		PlayerRace.crossedFinishLine += CrossedFinishLine;
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		TurnRibbonHandler.cardPlayedEvent -= CardPlayedEvent;
		PlayerSpell.cardPlayedByOpponentEvent -= CardPlayedByOpponentEvent;
		PlayerSpell.cardCanceledEvent -= CardCanceledEvent;
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	/// <summary>
	/// A card was played by the local player.
	/// Timers will only be added for Cards with a DURATION_WITH_TIMER property.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="level">Level.</param>
	void CardPlayedEvent( CardName name, int level )
	{
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		//Does this card have a DURATION_WITH_TIMER property?
		if( playedCard.doesCardHaveThisProperty( CardPropertyType.DURATION_WITH_TIMER ) )
		{
			string localizedName = LocalizationManager.Instance.getText( "CARD_NAME_" + name.ToString().ToUpper() );
			float duration = playedCard.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level );
			GameObject go = addTimer( localizedName, duration, Color.green );
			cardTimerDataList.Add( new CardTimerData( name, go, false ) );
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
		string localizedName = LocalizationManager.Instance.getText( "CARD_NAME_" + name.ToString().ToUpper() );
		GameObject go = addTimer( localizedName, duration, Color.red );
		cardTimerDataList.Add( new CardTimerData( name, go, true ) );
	}

	GameObject addTimer(string localizedName, float duration, Color color )
	{
		GameObject go = (GameObject)Instantiate(radialTimerPrefab);
		go.transform.SetParent(timerHolder,false);
		go.GetComponent<RadialTimerUI>().startAnimation( localizedName, duration, color );
		return go;
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			removeAllTimers();
		}
	}

	void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot )
	{
		//Only remove the timers if the player is not a bot.
		if( !isBot ) removeAllTimers();
	}

	void CardCanceledEvent( CardName name, bool playedByOpponent )
	{
		removeTimer( name, playedByOpponent );
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
		cardTimerDataList.Clear();
	}

	void removeTimer( CardName name, bool playedByOpponent )
	{
		CardTimerData cardTimerData = cardTimerDataList.Find(cardTimer => ( cardTimer.name == name && cardTimer.playedByOpponent == playedByOpponent ) );
		if( cardTimerData != null )
		{
			Debug.Log( "CardTimerHandler-removeTimer-Card Name: " + name + " playedByOpponent: " + playedByOpponent );
			//Destroy the radial timer on the HUD
			GameObject.Destroy( cardTimerData.radialTimerObject );
			cardTimerDataList.Remove( cardTimerData );
		}
	}

	public class CardTimerData
	{
		public CardName name;
		public GameObject radialTimerObject; 
		public bool playedByOpponent;

		public CardTimerData ( CardName name, GameObject radialTimerObject, bool playedByOpponent )
		{
			this.name = name;
			this.radialTimerObject = radialTimerObject;
			this.playedByOpponent = playedByOpponent;
		}
	}

}
