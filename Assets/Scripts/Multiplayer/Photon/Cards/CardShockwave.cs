using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Shockwave card is an Epic card with 8 levels. The shockwave causes everyone within radius/2 to dies and players between radius/2 and radius to stumble.
/// //To add a dramatic effect, it also makes all of the objects that have the Movable layer and a rigidbody to move because of the shockwave.
/// </summary>
public class CardShockwave : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardShockwaveMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardShockwaveMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Create shockwave
		object[] data = new object[2];
		data[0] = photonViewID;

		//Level related parameters
		data[1] = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );

		PhotonNetwork.InstantiateSceneObject( "Shockwave", playerTransform.TransformPoint( getSpawnOffset() ), playerTransform.rotation, 0, data );
	}
	#endregion
}
