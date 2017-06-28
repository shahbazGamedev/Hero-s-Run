using System.Collections;
using UnityEngine;

public class CardHyperFocus : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardHyperFocusMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHyperFocusMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		playerTransform.GetComponent<PhotonView>().RPC("cardHyperFocusRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level ), cd.getCardPropertyValue( CardPropertyType.SPEED_MULTIPLIER, level ) );
	}
	#endregion
}
