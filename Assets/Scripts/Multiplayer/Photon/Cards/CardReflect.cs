using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Reflect card is a Legendary card with 5 levels. Reflect certains spells back to the caster for a short while. Currently only works for Lightning.
/// If player A casts Lightning on player B, and player B has Reflect active, player A will be struck by his own Lightning.
/// </summary>
public class CardReflect : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardReflectMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardReflectMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		playerTransform.GetComponent<PlayerSpell>().cardReflectRPC( cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level ) );
	}
	#endregion
}
