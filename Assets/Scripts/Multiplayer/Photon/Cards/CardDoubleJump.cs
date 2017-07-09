using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Double Jump card is a Common card with 13 levels. The player will jump higher and further than normal.
/// </summary>
public class CardDoubleJump : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardDoubleJumpMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardDoubleJumpMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardDoubleJumpRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DOUBLE_JUMP_SPEED, level ), photonViewID );
	}
	#endregion

	[PunRPC]
	void cardDoubleJumpRPC( float doubleJumpSpeed, int photonViewID )
	{
		getPlayerControl( photonViewID ).jump( true, doubleJumpSpeed );
	}

}
