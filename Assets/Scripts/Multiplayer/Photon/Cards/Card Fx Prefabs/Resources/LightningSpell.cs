using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningSpell : CardSpawnedObject {
	
	[SerializeField] GameObject lightningTrail;
	[SerializeField] GameObject lightningDecal;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			//Read the data
			object[] data = gameObject.GetPhotonView ().instantiationData;
			casterTransform = getPlayerByViewID( (int) data[0] );
			setCasterName( casterTransform.name );
			float range = (float) data[1];
			positionSpawnedObject( 4f );
			strikeCreatures( range );
		}
		else
		{
			GameObject.Destroy( gameObject, 2.5f );
		}
	}

	void strikeCreatures( float range )
	{
		//Find one or more creatures to target that are within range.
		List<Transform> creatureList = getAllCreatureTransformsWithinRange( casterTransform, range );

		//Only continue if we found at least one target.
		if( creatureList.Count > 0 )
		{
			for( int i = 0; i < creatureList.Count; i++ )
			{
				//For each creature, spawn a lightning trail that starts at the Lightning System (which has a plasma ball effect) and ends on the torso of the target.
				GameObject go = GameObject.Instantiate( lightningTrail, transform );
				go.GetComponent<RFX4_ParticleTrail>().Target = creatureList[i].gameObject;

				//Place a burnt decal on the ground underneath the creature.
				Vector3 decalPosition = creatureList[i].TransformPoint( new Vector3(0,1f,0) );
                spawnDecalOnTheGround( lightningDecal, decalPosition, lightningDecal.transform.rotation, 10f );
			}
		}
		Destroy( gameObject, 10f );
	}

	List<Transform> getAllCreatureTransformsWithinRange( Transform caster, float range )
	{
		List<Transform> creatures = new List<Transform>();

		Collider[] hitColliders = Physics.OverlapSphere( caster.position, range, MaskHandler.getMaskOnlyCreatures() );
		Transform creature;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !isTargetValid( caster, hitColliders[i].transform ) ) continue;

			creature = hitColliders[i].transform;
			creatures.Add( creature );
		}
		return creatures;
	}

	bool isTargetValid( Transform caster, Transform target )
	{
		bool valid = false;
		ICreature creatureController = target.GetComponent<ICreature>();
		if( creatureController != null && creatureController.getCreatureState() != CreatureState.Dying && creatureController.getCreatureState() != CreatureState.Immobilized )
		{
			valid = true;
		}
		valid = valid && getDotProduct( caster, target );

 		//if( valid ) Debug.Log("isTargetValid " + target.name );
		return valid;
	}

	/// <summary>
	/// Returns true if the target is in front of the caster, false otherwise.
	/// </summary>
	/// <returns><c>true</c>, if the target is in front of the caster, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="potentialTarget">Potential target.</param>
	bool getDotProduct( Transform caster, Transform target )
	{
		Vector3 forward = caster.TransformDirection(Vector3.forward);
		Vector3 toOther = target.position - caster.position;
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

