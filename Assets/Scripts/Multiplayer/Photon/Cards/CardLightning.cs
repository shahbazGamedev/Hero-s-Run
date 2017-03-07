using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Lightning card is a Rare card with 11 levels.
/// Lightning strikes the nearest player, killing it instantly.
/// Because of its long range, it can be useful when an opponent is far away.
/// The spell range depends on the level of the caster.
/// </summary>
public class CardLightning : Photon.PunBehaviour {

	[SerializeField] float  baseRange = 50f;
	[SerializeField] float  rangeUpgradePerLevel = 10f;
	[SerializeField]  string lightningPrefabName = "Lightning";
	Vector3 offset = new Vector3( 0, 1f, 1f );

	int playerLayer = 2; //ignoreRaycast is the layer the player has. It avoids player raycasts from colliding with himself.

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardLightningMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardLightningMasterRPC( int level, int photonViewID )
	{
		//Find out which player activated the card
		GameObject playerGameObject = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerGameObject = (GameObject)PhotonNetwork.player.TagObject;
			}
		}

		//Calculate spell range
		float spellRange = baseRange + level * rangeUpgradePerLevel;

		//Get nearest target which could be either a player or creature
		Transform nearestTarget = detectNearestTarget( playerGameObject.transform, spellRange, photonViewID );

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

	Transform detectNearestTarget( Transform caster, float spellRange, int photonViewID )
	{
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
				//Ignore dead players
				if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying ) continue;
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
