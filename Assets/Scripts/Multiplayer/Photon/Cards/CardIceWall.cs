using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Ice Wall card is a Rare card with 11 levels. The ice wall appears a few meters in front of the player. The duration of the ice wall depends on the level of the caster.
/// The player can run through it.
/// </summary>
public class CardIceWall : Card {

	[SerializeField]  string prefabName = "IceWall";

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardIceWallMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardIceWallMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Spawn an ice wall a few meters in front of the player
		Vector3 objectPosition = playerTransform.TransformPoint( spawnOffset );

		Quaternion objectRotation = playerTransform.rotation;

		object[] data = new object[2];

		//We want the caster to be able to run through the ice wall.
		data[0] = playerTransform.name;

		//We want the ice wall to disappear after a while.
		data[1] = getDuration( level );

		PhotonNetwork.InstantiateSceneObject( prefabName, objectPosition, objectRotation, 0, data );
	}
	#endregion

}
