using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Sentry card is a Rare card with 11 levels. A sentry appears next to the player and zaps nearby opponents.
/// </summary>
public class CardSentry : Card {

	[SerializeField] float  baseAccuracy = 0.02f;
	[SerializeField] float  AccuracyUpgradePerLevel = -0.0018f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSentryMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSentryMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Create sentry
		Vector3 sentryOffsetToPlayer = new Vector3( 0.8f, 2.3f, 0 );
		Vector3 sentrySpawnPosition = playerTransform.TransformPoint( sentryOffsetToPlayer );

		object[] data = new object[4];
		data[0] = photonViewID;

		//Level related parameters
		data[1] = getDuration( level );
		data[2] = getRange( level );
		data[3] = baseAccuracy + level * AccuracyUpgradePerLevel;

		PhotonNetwork.InstantiateSceneObject( "sentry", sentrySpawnPosition, transform.rotation, 0, data );
	}
	#endregion

}
