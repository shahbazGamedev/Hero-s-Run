using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFrisbee : Card {

	[SerializeField]  string prefabName = "Frisbee";

	public void activateCard ( int photonViewId, int level )
	{
		spawnOffset = new Vector3( 0, 1f, 2f );
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardFrisbeeMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardFrisbeeMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Create frisbee
		object[] data = new object[2];
		data[0] = photonViewID;
		//We want the frisbee to self-destruct after a while
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
		PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( spawnOffset ), playerTransform.rotation, 0, data );

	}
	#endregion
}
