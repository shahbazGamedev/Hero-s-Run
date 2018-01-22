using System.Collections;
using UnityEngine;

/// To do:
/// Make sure it uses lockstep for card activation.
public class QuantumRift : CardSpawnedObject {

	const float HEIGHT_GROUND_OFFSET = 4f;
	[SerializeField] Transform rock;
	[SerializeField] MeshCollider rockCollider;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = gameObject.GetPhotonView ().instantiationData;
		
		casterTransform = getCaster( (int) data[0] );
		setCasterName( casterTransform.name );

		Transform target = getCaster( (int) data[2] );

		//We don't want the caster to be hit by the rock.
		//However, the caster could be the target if the opponent had the Reflect card active.
		if( target != null && casterTransform != null && casterTransform != target )
		{
			//Make the caster ignore collisions with the rock.
			Physics.IgnoreCollision( rockCollider, casterTransform.GetComponent<CapsuleCollider>() );
		}

		RaycastHit hit;
		int originalLayer = rock.gameObject.layer;
		rock.gameObject.layer = MaskHandler.ignoreRaycastLayer;
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y, transform.position.z ), Vector3.down, out hit, 25f ))
		{
			//Position above the ground at HEIGHT_GROUND_OFFSET
			transform.position = new Vector3( transform.position.x, hit.point.y + HEIGHT_GROUND_OFFSET, transform.position.z );
		}
		//Now that our raycast is finished, reset the rock's layer to its original value.
		rock.gameObject.layer = originalLayer;

		//Orient the rock that will be launched towards the target.
		if( !GameManager.Instance.isCoopPlayMode() )
		{
			 rock.LookAt( target );
		}
		Destroy( gameObject, 10f );
	}
	
	public void rockHitSomeone ( GameObject targetHit )
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
	 		if( targetHit.CompareTag("Zombie") )
			{
				//Zombie was hit by rock launched by rift. Knock him back.
				SkillBonusHandler.Instance.grantScoreBonus( 25, "COOP_SCORE_BONUS_QUANTUM_RIFT", casterTransform );
				ICreature creatureController = targetHit.GetComponent<ICreature>();
				creatureController.knockback( casterTransform, false );
			}
		}
 		else
		{
			if( targetHit.CompareTag("Player") )
			{
				addSkillBonus( 25, "SKILL_BONUS_QUANTUM_RIFT" );
				//Player was hit by rock launched by rift. Deduct some health. If the player was running, he will stumble.
				targetHit.GetComponent<PlayerHealth>().deductHealth( (int) gameObject.GetPhotonView ().instantiationData[1] );
			}
		}
	}
}
