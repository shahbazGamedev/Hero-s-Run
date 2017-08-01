using System.Collections;
using UnityEngine;

public class Shockwave : CardSpawnedObject {

	const float EXPLOSION_FORCE = 1500f;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		int casterViewId = (int) data[0];
		Transform caster = getCaster( casterViewId );

		//Do a ripple effect on the minimap
		//To do
		//MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

		if( caster != null ) shockwave( caster, (float) data[1] );

	}

	void shockwave( Transform caster, float radius )
	{
		float halfRadius = radius * 0.5f;

		//To add a dramatic effect, make all of the objects that have the Movable layer and a rigidbody move because of the shockwave.
		Collider[] movableHitColliders = Physics.OverlapSphere( transform.position, radius, MaskHandler.getMaskOnlyMovable() );
		for( int i = 0; i < movableHitColliders.Length; i++ )
		{
			if( movableHitColliders[i].attachedRigidbody != null )
			{
				movableHitColliders[i].attachedRigidbody.AddExplosionForce( EXPLOSION_FORCE, transform.position, halfRadius, EXPLOSION_FORCE );
			}
		}

		//Shake the camera
		caster.GetComponent<PlayerCamera>().Shake();

		//Nearby players dies. Players that are further stumble.
		Collider[] hitColliders = Physics.OverlapSphere( transform.position, radius, MaskHandler.getMaskOnlyPlayer() );
		for( int i = 0; i < hitColliders.Length; i++ )
		{
			//Ignore the caster
			if( hitColliders[i].transform == caster ) continue;

			//Ignore players in the Idle or Dying state
			if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle || hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying || hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Ziplining ) continue;

			float distance = Vector3.Distance( hitColliders[i].transform.position, transform.position );
			if( distance < halfRadius )
			{
				//Player is near. Kill him.
				topplePlayer( hitColliders[i].transform );
			}
			else
			{
				//Player is a bit further. Make him stumble.
				hitColliders[i].GetComponent<PlayerControl>().stumble();
			}
		}
	}

	void topplePlayer( Transform potentialTarget )
	{
		if( getDotProduct( potentialTarget, transform.position ) )
		{
			//Explosion is in front of player. He falls backward.
			potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.Obstacle );
		}
		else
		{
			//Explosion is behind player. He falls forward.
			potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.FallForward );
		}
	}

	bool getDotProduct( Transform player, Vector3 explosionLocation )
	{
		Vector3 forward = player.TransformDirection(Vector3.forward);
		Vector3 toOther = explosionLocation - player.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
}
