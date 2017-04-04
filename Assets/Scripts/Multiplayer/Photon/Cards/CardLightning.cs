﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Lightning card is an Epic card with 8 levels.
/// Lightning strikes the nearest player, killing it instantly.
/// Because of its long range, it can be useful when an opponent is far away.
/// The spell range depends on the level of the caster.
/// </summary>
public class CardLightning : Card {

	[SerializeField] float  baseRange = 50f;
	[SerializeField] float  rangeUpgradePerLevel = 10f;
	[SerializeField]  string lightningPrefabName = "Lightning";
	Vector3 offset = new Vector3( 0, 1f, 1f );

	int playerLayer = 8;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardLightningMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardLightningMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Get nearest target which could be either a player or creature
		Transform nearestTarget = detectNearestTarget( playerTransform, level, photonViewID );

		if( nearestTarget != null )
		{
			//Spawn a lightning on the nearest player or creature
			Vector3 lightningPosition = nearestTarget.transform.TransformPoint( offset );
			PhotonNetwork.InstantiateSceneObject( lightningPrefabName, lightningPosition, nearestTarget.rotation, 0, null );
	
			//Kill nearest target
			strike( nearestTarget );
		}
		else
		{
			Debug.Log("CardLightning: no target found." );
		}
	}

	int getOverlapSphereMask()
	{
		int mask = 1 << playerLayer;
		return mask;
	}

	public Transform detectNearestTarget( Transform caster, int level, int photonViewID )
	{
		float spellRange = baseRange + level * rangeUpgradePerLevel;
		//Use a sphere centered around the caster
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, spellRange, getOverlapSphereMask() );
		float nearestDistance = 100000;
		Transform nearestTarget = null;
		
		//Keep nearest target only
		for( int i =0; i < hitColliders.Length; i++ )
		{
			float distanceToTarget = Vector3.Distance( caster.position, hitColliders[i].transform.position );
			if(  hitColliders[i].transform != caster && distanceToTarget < nearestDistance )
			{
				//Make sure it is a player before continuing
				if( !hitColliders[i].gameObject.CompareTag("Player") ) continue;
				//Ignore dead players or players that are idle
				if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying || hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;
				nearestTarget = hitColliders[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	void strike( Transform nearestTarget )
	{
		nearestTarget.GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.Obstacle );
	}

	#endregion

}
