using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Supercharger card is a Epic card with 8 levels. Supercharge your omni-tool and play all cards as if they were 2 levels higher without exceeding the maximum level allowed for that card.
/// </summary>
public class CardSupercharger : Card {

	public const int SUPERCHARGER_LEVEL_BOOST = 2;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSuperchargerMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSuperchargerMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		playerTransform.GetComponent<PhotonView>().RPC("cardSuperchargerRPC", PhotonTargets.All, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
	}
	#endregion

}
