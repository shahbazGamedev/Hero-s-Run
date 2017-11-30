using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Force Field card is a Rare card with 11 levels. The force field appears a few meters in front of the player. The duration and height depends on the level of the card.
/// The player can run through it on creation.
/// </summary>
public class CardForceField : Card {

	[SerializeField]  string prefabName = "Force Field";

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardForceFieldMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardForceFieldMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		object[] data = new object[3];

		//Caster PhotonView ID.
		data[0] = photonViewID;

		//We want the force field to disappear after a while.
		data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );

		//Height.
		data[2] = cd.getCardPropertyValue( CardPropertyType.HEIGHT, level );


		PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( getSpawnOffset() ), playerTransform.rotation, 0, data );
	}
	#endregion

}
