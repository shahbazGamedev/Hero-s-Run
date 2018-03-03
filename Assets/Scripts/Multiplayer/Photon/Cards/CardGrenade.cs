using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The Grenade card is a Rare card with 11 levels. It detonates automatically after about 1 second. The player is immune to its effect.
/// </summary>
public class CardGrenade : Card {

	[SerializeField]  string prefabName;
	const float GRENADE_RANGE = 25f;

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardGrenadeMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardGrenadeMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		object[] data = new object[2];

		//We want the caster to be immune to the blast
		data[0] = photonViewID;

		//We want to transmit the blast radius
		data[1] = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );

		Vector3 spawnPosition;
		if( GameManager.Instance.isCoopPlayMode() )
		{
			Vector3 groupCenter = getCentroid( playerTransform );
			if( groupCenter == Vector3.zero )
			{
				spawnPosition = playerTransform.TransformPoint( getSpawnOffset() );
			}
			else
			{
				spawnPosition = groupCenter + new Vector3( 0, 1.3f, 0 );
			}	
		}
		else
		{
			spawnPosition = playerTransform.TransformPoint( getSpawnOffset() );
		}

		//Drop the grenade. It has a rigidbody and hence it has gravity and will fall to the ground.
		PhotonNetwork.InstantiateSceneObject( prefabName, spawnPosition, playerTransform.rotation, 0, data );
	}
	#endregion

	#region Coop
	/// <summary>
	/// Returns the center of a group of creatures which are within range and in front of the caster.
	/// If there are no such creatures, will return Vector3.zero.
	/// </summary>
	/// <returns>The group center.</returns>
	/// <param name="playerTransform">Player transform.</param>
	Vector3 getCentroid( Transform playerTransform )
	{
		//Find one or more creatures that are near the player.
		List<Transform> creatureList = getAllCreatureTransformsWithinRange( playerTransform, GRENADE_RANGE );

		//If there are no creatures, return.
		if( creatureList.Count == 0 ) return Vector3.zero;

		//We want to spawn the grenade in the center of the creatures that are in front of the player only.
		List<Transform> creatureListInFront = new List<Transform>(creatureList.Count);
		for( int i = 0; i < creatureList.Count; i++ )
		{
			if( getDotProduct( playerTransform, creatureList[i] ) ) creatureListInFront.Add( creatureList[i] ) ;
		}

		//If there are no creatures in front of the player, return.
		if( creatureListInFront.Count == 0 ) return Vector3.zero;

		Vector3 groupVectors = Vector3.zero;
		for( int i = 0; i < creatureListInFront.Count; i++ )
		{
			groupVectors += creatureListInFront[i].position;
		}
		return groupVectors/(float)creatureListInFront.Count;
	}
	#endregion

}
