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
		
		//Destroy the force field when it expires
		float duration = (float) data[0];
		StartCoroutine( destroySpawnedObject( duration, DELAY_BEFORE_DESTROY_EFFECTS ) );

		//Adjust the height
		float height = (float) data[1];
		transform.localScale = new Vector3( 8f, height, 1f );

		//Display the force field icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position it flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( height * 0.5f );

		//We can now make the force field visible and collidable
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<BoxCollider>().enabled = true;

		//The activation delay is so the caster can run through it before it becomes solid.
		yield return new WaitForSeconds(delayBeforeActivation);

		setSpawnedObjectState(SpawnedObjectState.Functioning);

		//Make it solid.
		GetComponent<BoxCollider>().isTrigger = false;

		//Play looping particle effect
		activeForceFieldFx.Play();
	}

	void OnCollisionEnter(Collision collision)
	{
		//Play collision sound
		GetComponent<AudioSource>().PlayOneShot( collisionSound );
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