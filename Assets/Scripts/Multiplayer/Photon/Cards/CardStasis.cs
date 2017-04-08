using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Stasis card is a Common card with 13 levels.
/// The spell traps the player in a stasis force field for the spell duration, which depends on the level of the card.
/// The player is immune to other spells while in stasis.
/// The spell range depends on the level of the card.
/// </summary>
public class CardStasis : Card {

	[SerializeField]  string prefabName = "Statis";

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardStasisMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardStasisMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Find the nearest target
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), getRange( level ) );

		//Only continue if we found a target
		if( nearestTarget != null )
		{
			object[] data = new object[2];
	
			//We will need to find a reference to the player we are targeting
			data[0] = nearestTarget.GetComponent<PhotonView>().viewID;
	
			//We want the stasis sphere to disappear after a while
			data[1] = getDuration( level );
	
			PhotonNetwork.InstantiateSceneObject( prefabName, nearestTarget.position, nearestTarget.rotation, 0, data );
		}
		else
		{
			Debug.Log("CardStasis - No target found.");
		}
	}
	#endregion

}
