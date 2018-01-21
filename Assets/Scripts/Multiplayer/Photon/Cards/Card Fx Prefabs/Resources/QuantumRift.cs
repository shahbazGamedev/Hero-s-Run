using System.Collections;
using UnityEngine;

public class QuantumRift : CardSpawnedObject {

	const float HEIGHT_GROUND_OFFSET = 4f;
	[SerializeField] Transform rock;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = gameObject.GetPhotonView ().instantiationData;
		
		casterTransform = getCaster( (int) data[0] );
		setCasterName( casterTransform.name );

		RaycastHit hit;
		int originalLayer = gameObject.layer;
		gameObject.layer = MaskHandler.ignoreRaycastLayer;
		//Important: we need the +2f to start a bit above ground, or else the raycast may start a tad below the road and miss.
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + 2f, transform.position.z ), Vector3.down, out hit, 25f ))
		{
			//Position it flush with the ground
			transform.position = new Vector3( transform.position.x, hit.point.y + HEIGHT_GROUND_OFFSET, transform.position.z );
		}
		//Now that our raycast is finished, reset the object's layer to its original value.
		gameObject.layer = originalLayer;

		//Orient the rock that will be launched to the target.
		if( !GameManager.Instance.isCoopPlayMode() )
		{
			 rock.LookAt( getCaster( (int) data[2] ) );
		}
		Destroy( gameObject, 10f );
	}
	
	public void rockHitSomeone ( GameObject targetHit )
	{
		//Play collision sound
		//GetComponent<AudioSource>().PlayOneShot( collisionSound );
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
				if( targetHit.name != casterName )
				{
					addSkillBonus( 25, "SKILL_BONUS_QUANTUM_RIFT" );
					//Player was hit by rock launched by rift. Deduct some health. If the player was running, he will stumble.
					targetHit.GetComponent<PlayerHealth>().deductHealth( (int) gameObject.GetPhotonView ().instantiationData[1] );
				}
			}
		}
	}
}
