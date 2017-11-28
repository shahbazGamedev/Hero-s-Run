using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Trip mine.
/// For the OnTriggerEnter method to be called, the BoxCollider component which is used as the trigger must be above the MeshCollider component.
/// </summary>
public class TripMine : CardSpawnedObject {
	
	[SerializeField] ParticleSystem explosionEffect;
	float blastRadius;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Trip Mine prefab has its MeshRenderer disabled and trigger box collider.
		//We will enable them only when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Trip_Mine );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterTransform = getCaster( (int) data[0] );
		setCasterName( casterTransform.name );

		float delayBeforeExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeExpires );

		blastRadius = (float) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the trip mine flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 0 );

		//We can now make the trip mine visible and triggerable
		GetComponent<BoxCollider>().enabled = true;
		transform.Find("Sci-fi Mine base").GetComponent<MeshRenderer>().enabled = true;
		transform.Find("Sci-fi Mine leg").GetComponent<MeshRenderer>().enabled = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
	 		if( other.CompareTag("Zombie") )
			{
				startDetonationCountdown();
				SkillBonusHandler.Instance.grantScoreBonus( 25, "COOP_SCORE_BONUS_TRIP_MINE", casterTransform );
			}
		}
 		else
		{
			if( other.CompareTag("Player") )
			{
				if( other.name != casterName )
				{
					startDetonationCountdown();
					addSkillBonus( 25, "SKILL_BONUS_TRIP_MINE" );
				}
			}
		}
	}

	void startDetonationCountdown()
	{
		GetComponent<AudioSource>().Play();
		Invoke( "detonate", 0.94f ); //the bomb beeps lasts 0.94 seconds
	}

	void detonate()
	{
		destroyAllTargetsWithinBlastRadius( blastRadius, MaskHandler.getMaskAll(), casterTransform );
		explode();
		GameObject.Destroy( gameObject );
	}

	void explode()
	{
		ParticleSystem effect = GameObject.Instantiate( explosionEffect, transform.position, transform.rotation );
		effect.GetComponent<AudioSource>().Play ();
		effect.Play();
		//Destroy the particle effect after a few seconds
		GameObject.Destroy( effect.gameObject, 3f );
	}
}