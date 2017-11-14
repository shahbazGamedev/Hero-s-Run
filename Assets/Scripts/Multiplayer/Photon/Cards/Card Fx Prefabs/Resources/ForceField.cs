using UnityEngine;
using System.Collections;

public class ForceField : CardSpawnedObject {
	
	[SerializeField] ParticleSystem activeForceFieldFx; //Used to visually indicate that the force field is active
	[SerializeField] AudioClip collisionSound;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Force Field prefab has its MeshRenderer and BoxCollider disabled.
		//We will enable them when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Force_Field );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		StartCoroutine( activate( 0.8f ) );
	}

	IEnumerator activate( float delayBeforeActivation )
	{
		//Read the data
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		
		casterTransform = getCaster( (int) data[0] );
		setCasterName( casterTransform.name );

		//Destroy the force field when it expires
		float duration = (float) data[1];
		StartCoroutine( destroySpawnedObject( duration, DELAY_BEFORE_DESTROY_EFFECTS ) );

		//Adjust the height
		float height = (float) data[2];
		transform.localScale = new Vector3( 0.1f, height, 1f );

		//Display the force field icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position it flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( height * 0.5f );

		//We can now make the force field visible and collidable
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<BoxCollider>().enabled = true;
		LeanTween.scaleX( gameObject, 8f, 0.33f ).setEaseOutQuart();

		//The activation delay is so the caster can run through it before it becomes solid.
		yield return new WaitForSeconds(delayBeforeActivation);

		setSpawnedObjectState(SpawnedObjectState.Functioning);

		//Make it solid.
		GetComponent<BoxCollider>().isTrigger = false;

		//Play looping particle effect
		activeForceFieldFx.Play();

		//Play energy sound loop
		GetComponent<AudioSource>().Play();
	}

	void OnCollisionEnter(Collision collision)
	{
		//Play collision sound
		GetComponent<AudioSource>().PlayOneShot( collisionSound );
		if( collision.collider.CompareTag("Player") )
		{
			if( collision.collider.name != casterName )
			{
				addSkillBonus( 25, "SKILL_BONUS_FORCE_FIELD" );
			}
		}
 		else if( collision.collider.CompareTag("Zombie") )
		{
			addSkillBonus( 25, "SKILL_BONUS_FORCE_FIELD" );
			ICreature creatureController = collision.collider.GetComponent<ICreature>();
			creatureController.knockback();
		}
 	}

	public override void destroySpawnedObjectNow()
	{
		StartCoroutine( destroySpawnedObject( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySpawnedObject( float delayBeforeExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeExpires);
		GetComponent<BoxCollider>().isTrigger = true;
		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		StopCoroutine( "activate" );
		activeForceFieldFx.Stop();
		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		Destroy( gameObject );
	}
}