using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Sentry card is a Rare card with 11 levels. A sentry appears next to the player and zaps nearby opponents.
/// </summary>
public class CardSentry : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSentryMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSentryMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Create sentry
		object[] data = new object[4];
		data[0] = photonViewID;

		//Level related parameters
		data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level );
		data[2] = cd.getCardPropertyValue( CardPropertyType.AIM_RANGE, level );
		data[3] = cd.getCardPropertyValue( CardPropertyType.ACCURACY, level );

		PhotonNetwork.InstantiateSceneObject( "Sentry", playerTransform.position, transform.rotation, 0, data );
	}
	#endregion

}
