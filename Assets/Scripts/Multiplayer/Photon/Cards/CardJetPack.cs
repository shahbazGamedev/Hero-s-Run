using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardJetPack : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardJetPackMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardJetPackMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		playerTransform.GetComponent<PhotonView>().RPC("cardJetPackRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level ), cd.getCardPropertyValue( CardPropertyType.FLIGHT_SPEED, level ) );
	}
	#endregion
}
