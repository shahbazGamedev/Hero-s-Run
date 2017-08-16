using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Smoke Bomb card is a Common card with 13 levels. It detonates automatically after about 1 second. The player is immune to its effect.
/// </summary>
public class CardSmokeBomb : Card {

	[SerializeField]  string prefabName;
	Vector3 offset = new Vector3( 0, 1.3f, 6f );

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardSmokeBombMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSmokeBombMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		object[] data = new object[1];

		//We want to transmit the duration
		data[0] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );

		//Drop the smoke bomb in front of the player at arm's length. It has a rigidbody and hence it has gravity and will fall to the ground.
		//We add 5 degrees in X so that the grenade topples.
		PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( offset ), Quaternion.Euler( 5f, playerTransform.rotation.y, 0 ), 0, data );
	}
	#endregion

}
